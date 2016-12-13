using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace BattleAxe
{
    public static class ParameterMethods
    {
        //needs retesting
        internal static void SetInputs<T>(T sourceForInputParameters, SqlCommand command, bool shipStructured = false)
            where T : class
        {
            if (sourceForInputParameters != null  && command?.Parameters.Count > 0)                
            {
                insureSourceColumnExists(command.Parameters);
                Func<T, string, object> getMethod = Compiler.GetMethod(sourceForInputParameters);
                try
                {
                    foreach (SqlParameter parameter in command.Parameters)
                    {
                        if (parameter.SqlDbType != SqlDbType.Structured)
                        {
                            if (parameter.Direction == ParameterDirection.Input ||
                                parameter.Direction == ParameterDirection.InputOutput)
                            {
                                var value = getMethod(sourceForInputParameters, parameter.SourceColumn);
                                parameter.Value = value != null ? value : DBNull.Value;
                            }
                        }
                        else if (!shipStructured)
                        {
                            var structureObj = getMethod(sourceForInputParameters, parameter.SourceColumn);
                            parameter.Value = GetDataTable(structureObj, command, parameter);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        static void insureSourceColumnExists(SqlParameterCollection parameters) {
            if (parameters?.Count > 0 && string.IsNullOrEmpty(parameters[1].SourceColumn)) {
                foreach (SqlParameter item in parameters) {
                    item.SourceColumn = item.ParameterName.Replace("@", "");
                }
            }
        }
                
        internal static DataTable GetDataTable<T>(T referenceObject, SqlCommand command, SqlParameter parameter)
            where T : class
        {
            DataTable ret = new System.Data.DataTable();
            if (referenceObject != null)
            {
                var reference = CommandMethods.structureFields.Where(i => i.Item1.CommandText == command.CommandText && i.Item1.Connection.ConnectionString == command.Connection.ConnectionString && i.Item2 == parameter.TypeName).ToList();
                foreach (var item in reference)
                {
                    if (!ret.Columns.Contains(item.Item3)) {
                        ret.Columns.Add(item.Item3);
                    }
                }
                var type = referenceObject.GetType();
                if (type.Name == "List`1")
                {
                    IList data = (IList)referenceObject;
                    if (data.Count > 0)
                    {
                        Func<object, string, object> getMethod;
                        if (data[0] is IBattleAxe)
                        {
                            getMethod = (o, s) => ((IBattleAxe)o)[s];                           
                        }
                        else {
                            var tempMethod = Compiler.GetMethod2(data[0]);
                            getMethod = (o, s) => tempMethod(o, s);
                        }                        
                        foreach (var obj in data)
                        {
                            DataRow row = ret.NewRow();
                            foreach (var item in reference)
                            {
                                var value = getMethod(obj, item.Item3);
                                if (value is Enum)
                                {
                                    row[item.Item3] = (int)value;
                                }
                                else
                                {
                                    row[item.Item3] = value;
                                }
                            }
                            ret.Rows.Add(row);
                        }
                    }
                }
                else
                {
                    Func<T, string, object> getMethod = Compiler.GetMethod(referenceObject);
                    DataRow row = ret.NewRow();
                    foreach (var item in reference)
                    {
                        row[item.Item3] = getMethod(referenceObject, item.Item3);
                    }
                    ret.Rows.Add(row);
                }
            }
            return ret;
        }

        internal static void SetOutputs<T>(T targetForOutputParameters, SqlCommand command)
            where T : class
        {
            try
            {
                var setMethod = Compiler.SetMethod(targetForOutputParameters);
                foreach (SqlParameter p in command.Parameters)
                {
                    if (p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.InputOutput)
                    {
                        object value = p.Value;
                        if (value == DBNull.Value)
                        {
                            value = null;
                        }
                        var field = !string.IsNullOrEmpty(p.SourceColumn) ? p.SourceColumn : p.ParameterName.Replace("@", "");
                        setMethod(targetForOutputParameters, field, value);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
