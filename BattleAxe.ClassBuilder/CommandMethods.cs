using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace BattleAxe.Class {

    public interface ICommandDefinition {
        string ConnectionString { get; set; }
        string CommandText { get; set; }
        CommandType CommandType { get; set; }
        int CommandTimeout { get; set; }
    }
    
    public class CommandDefinition : ICommandDefinition {
        private string connectionString;
        public string ConnectionString {
            get {
                if (string.IsNullOrEmpty(connectionString)) {
                    throw new System.InvalidOperationException("Command defintion requires connection string.");
                }
                return connectionString;
            }
            set { connectionString = value; }
        }

        private string commandText;

        public string CommandText {
            get {
                if (string.IsNullOrEmpty(commandText)) {
                    throw new System.InvalidOperationException("Command definition requires command text.");
                }
                return commandText;
            }
            set { commandText = value; }
        }

        public int CommandTimeout { get; set; } = 30;

        public CommandType CommandType { get; set; }

        /// <summary>
        /// If you supply the command text in form of schemaName.procedureName the command type will
        /// be set to StoredProcedure
        /// If you do not set the command type the process will look at sys.objects and 
        /// try to find the command text on the server database.  If it finds multiple schemas it wont know which 
        /// one to pick and will throw an exception.  If it doesnt find anything it will assume the 
        /// command text is Text based command.  If you make an inline sql statement with @ParameterName
        /// within the text the command parameters will be derived.
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="connectionString"></param>
        /// <param name="commandType"></param>
        public CommandDefinition(string commandText, string connectionString, CommandType? commandType = null) {
            this.commandText = commandText;
            this.connectionString = connectionString;
            setCommandType(commandType);
        }

        void setCommandType(CommandType? commandType) {
            if (!commandType.HasValue) {
                var pattern = "^\\w+\\.";
                if (System.Text.RegularExpressions.Regex.IsMatch(this.CommandText, pattern)) {
                    this.CommandType = System.Data.CommandType.StoredProcedure;
                }
                else {
                    setCommandText();
                }
            }
            else {
                this.CommandType = commandType.Value;
            }
        }

        protected void setCommandText() {
            var existsOnServer = getSchemaAndProcedure();
            if (existsOnServer.Count == 1) {
                this.CommandText = $"{existsOnServer[0].Item1}.{this.CommandText}";
                this.CommandType = System.Data.CommandType.StoredProcedure;
            }
            else if (existsOnServer.Count > 0) {
                throw new Exception($"Procedure {this.CommandText} could not be determined their are multiple procedures with same name across the schemas.");
            }
            else {
                this.CommandType = CommandType.Text;
            }
        }

        protected List<Tuple<string, string>> getSchemaAndProcedure() {
            List<Tuple<string, string>> ret = new List<Tuple<string, string>>();
            using (var connection = new SqlConnection(connectionString)) {
                using (var command = new SqlCommand(selectForProcedure(commandText), connection)) {
                    connection.Open();
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            ret.Add(new Tuple<string, string>(reader.GetString(0), reader.GetString(1)));
                        }
                    }
                }
            }
            return ret;
        }


        string selectForProcedure(string commandText) {
            return $@"select
                        s.name schemaName,
                        o.name procedureName
                      from
                        sys.objects o inner join
                        sys.schemas s on o.schema_id = s.schema_id
                      where
                        s.name != 'sys' and
                        o.is_ms_shipped = 0 and
                        o.name = '{commandText}' and
                        type in ('P')";
        }

    }

    public enum SqlCommandCacheTimeout {
        NeverExpires = -1,
        IsNeverCached = 0,
        FifteenMinutes = 15,
        Hour = 60,
        TwoHours = 120,
        FourHours = 240,
        EightHours = 480,
        Day = 1440,
        Week = 10080
    }

    public static class CommandMethods {
        public static SqlCommandCacheTimeout SqlCommandCacheTimeout { get; set; } = SqlCommandCacheTimeout.Day;        
        internal static List<Tuple<SqlCommand, string, string>> StructureFields { get; set; } = 
            new List<Tuple<SqlCommand, string, string>>();

        /// <summary>
        /// this will derive the parameters if this is a StoredProcedure type of command
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="connectionString"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static SqlCommand GetCommand(this string commandText, string connectionString, int commandTimeout = 30,
            CommandType commandType = CommandType.StoredProcedure) {


            using (var conn = new SqlConnection(connectionString)) {
                var sqlCommand = new SqlCommand {
                    CommandText = commandText,
                    CommandType = commandType,
                    CommandTimeout = commandTimeout
                };

                if (commandType == CommandType.StoredProcedure) {
                    deriveParametersForProcedure(commandText, connectionString, sqlCommand);
                }
                else {
                    deriveParametersForInlineCommand(sqlCommand);
                }
                return sqlCommand;
            }

        }

        public static SqlCommand GetCommand(this ICommandDefinition definition) => 
            definition.CommandText.GetCommand(definition.ConnectionString, definition.CommandTimeout, definition.CommandType);
        
        private static void deriveParametersForProcedure(string commandText, string connectionString, SqlCommand sqlCommand) {            
            using (var connection = new SqlConnection(connectionString)) {
                using (var temp = new SqlCommand(commandText, connection) { CommandType = CommandType.StoredProcedure }) {
                    connection.Open();
                    SqlCommandBuilder.DeriveParameters(temp);
                    foreach (SqlParameter p in temp.Parameters) {
                        var typeName = p.TypeName;
                        if (typeName != null && typeName.Count(c => c == '.') == 2) {
                            typeName = typeName.Substring(typeName.IndexOf(".") + 1);
                        }
                        sqlCommand.Parameters.Add(new SqlParameter {
                            Direction = p.Direction,
                            ParameterName = p.ParameterName,
                            SqlDbType = p.SqlDbType,
                            Size = p.Size,
                            Precision = p.Precision,
                            Scale = p.Scale,
                            SourceColumn = p.ParameterName.Replace("@", ""),
                            TypeName = typeName ?? null
                        });
                        if (p.SqlDbType == SqlDbType.Structured) {
                            addStructureFieldForParameter(sqlCommand, typeName, connectionString);
                        }
                    }
                }
            }
        }

        private static void addStructureFieldForParameter(SqlCommand referenceCommand, string typeName, string connectionString) {
            var commandString =
$@"select 
    c.name FieldName
from
    sys.table_types tt inner
join
sys.columns c on c.object_id = tt.type_table_object_id
where
    USER_NAME(tt.schema_id) + '.' + tt.name = '{ typeName }'";

            using (var conn = new SqlConnection(connectionString)) {
                using (var command = new SqlCommand(commandString, conn)) {
                    conn.Open();
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            string value = reader.GetString(0);
                            StructureFields.Add(new Tuple<SqlCommand, string, string>(referenceCommand, typeName, value));
                        }
                    }
                }
            }

        }

        private static void deriveParametersForInlineCommand(SqlCommand sqlCommand) {
            sqlCommand.CommandType = CommandType.Text;
            var regex = new System.Text.RegularExpressions.Regex("@\\w+");
            var matches = regex.Matches(sqlCommand.CommandText);
            foreach (System.Text.RegularExpressions.Match match in matches) {
                var parameterName = match.Value;
                if (!sqlCommand.Parameters.Contains(parameterName)) {
                    sqlCommand.Parameters.Add(new SqlParameter {
                        ParameterName = parameterName,
                        SourceColumn = parameterName.Replace("@", "")
                    });
                }
            }
        }
        
    }
}
