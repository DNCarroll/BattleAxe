using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BattleAxe.Class {
    public class Creator {

        public Creator() { }

        public Creator(CommandDefinition definition) {
            this.Definition = definition;
            setFields();
        }

        public string ClassName { get; set; }
        public string NameSpace { get; set; }
        public CommandDefinition Definition { get; set; }

        List<FieldDefintion> fields;



        public bool Initialized() {
            var form = new Form2();
            if(form.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                this.Definition = new CommandDefinition(form.CommandText.Text, form.GetConnectionString());
                this.NameSpace = form.NameSpace.Text;
                this.ClassName = form.ClassName.Text;
                //now build it
                //gotta get the innards to something use replace on a class item?
                //that will work   
                return true;
            }
            return false;
        }


        void setFields() {

            try {
                using (var conn = new SqlConnection(Definition.ConnectionString)) {
                    using (var cmd = Definition.GetCommand()) {
                        cmd.Connection = conn;

                        if (cmd.Parameters != null && cmd.Parameters.Count > 0) {
                            foreach (SqlParameter parameter in cmd.Parameters) {
                                parameter.Value = DBNull.Value;
                            }
                        }
                        conn.Open();
                        using (var reader = cmd.ExecuteReader()) {
                            fields = getFields(reader);
                        }
                    }
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }


        public string BuildTester() {
            try {
                using (var conn = new SqlConnection(Definition.ConnectionString)) {
                    using (var cmd = Definition.GetCommand()) {
                        cmd.Connection = conn;

                        if (cmd.Parameters != null && cmd.Parameters.Count > 0) {
                            foreach (SqlParameter parameter in cmd.Parameters) {
                                parameter.Value = DBNull.Value;
                            }
                        }
                        conn.Open();
                        using (var reader = cmd.ExecuteReader()) {
                            fields = getFields(reader);
                            var properties = Properties();
                            return getClass(properties, fields);
                        }
                    }

                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
            return "";
        }

        public string getClass(string properties, List<FieldDefintion> fields) {
            var classValue = @"using System;
using BattleAxe;

namespace {NameSpace} {
        public class {ClassName} : IBattleAxe {
            public object this[string property] {
                get {
                    switch (property){
                        {GetCaseStatements}
                        default:
                            return null;
                    }
                }
                set {
                    switch (property){
                        {SetCaseStatements}
                        default:
                            break;
                    }
                }
            }

            {Properties}
        }
    }";
            classValue = classValue.Replace("{NameSpace}", this.NameSpace);
            classValue = classValue.Replace("{ClassName}", this.ClassName);
            classValue = classValue.Replace("{Properties}", properties);
            classValue = classValue.Replace("{SetCaseStatements}", SetCaseStatemetns());
            classValue = classValue.Replace("{GetCaseStatements}", GetCaseStatements());
            return classValue;
        }

        public string SetCaseStatemetns() =>
            string.Join("\r\n", fields.Where(f => f.SetForIndexer != null).Select(f => f.SetForIndexer).ToArray());

        public string GetCaseStatements()=>
            string.Join("\r\n", fields.Select(f => f.GetForIndexer()).ToArray());

        public string Properties() =>            
            string.Join("\r\n", fields.Select(f => $"public {f.Type} {f.Name} " + " { get; set; }").ToArray());
        

        List<FieldDefintion> getFields(System.Data.SqlClient.SqlDataReader reader) {
            var ret = new List<FieldDefintion>();
            var fieldCount = reader.FieldCount;
            var fieldIndex = 0;
            while (fieldIndex < fieldCount) {
                ret.Add(new FieldDefintion(reader.GetName(fieldIndex), reader.GetDataTypeName(fieldIndex)));
                fieldIndex++;
            }
            return ret;
        }

    }

    public class FieldDefintion {

        public FieldDefintion(string name, string type) {
            this.Name = name;
            setType(type);
            
            switch (this.Type) {
                case "Int32":
                case "int":
                    this.SetForIndexer = caseStatement("Convert.ToInt32(value)", "0");
                    break;
                case "string":
                    this.SetForIndexer = caseStatement("Convert.ToString(value)", "System.String.Empty");
                    break;
                case "bool":
                    this.SetForIndexer = caseStatement("Convert.ToBoolean(value)", "false");
                    break;
                case "double":
                    this.SetForIndexer = caseStatement("Convert.ToDouble(value)", "0");
                    break;
                case "byte":
                    this.SetForIndexer = caseStatement("Convert.ToByte(value)", "(byte)0");
                    break;
                case "short":
                    this.SetForIndexer = caseStatement("Convert.ToInt16(value)", "(short)0");
                    break;
                case "long":
                    this.SetForIndexer = caseStatement("Convert.ToInt64(value)", "0");
                    break;
                case "Single":
                    this.SetForIndexer = caseStatement("Convert.ToSingle(value)", "0");
                    break;
                case "Decimal":
                case "decimal":
                    this.SetForIndexer = caseStatement("Convert.ToDecimal(value)", "0");
                    break;
                case "char":
                    this.SetForIndexer = caseStatement("Convert.ToChar(value)", "System.Char.MinValue");
                    break;
                case "Guid":
                    this.SetForIndexer = caseStatement("new System.Guid(value.ToString())", "System.Guid.Empty");
                    break;
                case "DateTime":
                    this.SetForIndexer = caseStatement("Convert.ToDateTime(value)", "System.DateTime.MinValue");
                    break;
                case "byte[]":
                    this.SetForIndexer = $"case \"{this.Name}\": this.{this.Name} = value!=null && value is byte[] ? (byte[])value: null; break;";
                    break;
                default:
                    break;
            }
            
        }

        void setType(string type) {

            switch (type) {
                case "int":
                    this.Type = type;
                    break;
                case "bit":
                    this.Type = "bool";
                    break;
                case "smallint":
                    this.Type = "short";
                    break;
                case "tinyint":
                    this.Type = "byte";
                    break;
                case "smallmoney":
                case "money":
                case "decimal":
                case "numeric":
                    this.Type = "Decimal";
                    break;
                case "real":
                    this.Type = "Single";
                    break;
                case "float":
                    this.Type = "double";
                    break;
                case "smalldatetime":
                case "date":
                case "datetime":
                case "datetime2":
                    this.Type = "DateTime";
                    break;
                case "uniqueidentifier":
                    this.Type = "Guid";
                    break;
                case "datetimeoffset":
                    this.Type = "DateTimeOffset";
                    break;
                case "text":
                case "char":
                case "nchar":
                case "nvarchar":
                case "varchar":
                case "ntext":
                    this.Type = "string";
                    break;
                case "bigint":
                    this.Type = "long";
                    break;
                case "varbinary":
                case "timestamp":
                case "rowversion":
                case "image":
                case "binary":
                    this.Type = "byte[]";
                    break;
                case "xml":
                    this.Type = "Xml";
                    break;
                default:
                    this.Type = type;
                    break;
            }

        }
        public string Name { get; set; }
        public string Type { get; set; }

        public string SetForIndexer { get; set; }

        public string GetForIndexer() {
            return $"case \"{this.Name}\": return {this.Name};";
        }
        string caseStatement(string convertMethod, string defaultValue = null) {
            var ret = $"case \"{this.Name}\": this.{this.Name} = value is {this.Type} ? ({this.Type})value : " +
                (defaultValue != null ?
                    $"value != null ? {convertMethod} : {defaultValue};" :
                    $"{convertMethod};"
                ) + "break;";
            return ret;
        }

    }
}
