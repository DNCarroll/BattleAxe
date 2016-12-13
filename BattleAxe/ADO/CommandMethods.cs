using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace BattleAxe {
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
        internal static List<SqlCommandCacheObject> cachedCommands { get; set; } = new List<SqlCommandCacheObject>();
        internal static List<Tuple<SqlCommand, string, string>> structureFields { get; set; } = 
            new List<Tuple<SqlCommand, string, string>>();

        /// <summary>
        /// this will derive the parameters if this is a StoredProcedure type of command
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="connectionString"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static SqlCommand GetCommand(this string commandText, string connectionString, CommandType commandType = CommandType.StoredProcedure) {
            connectionString = ConnectionMaintenance.ConnectionStringTimeout(connectionString);
            var found = getFromCache(commandText, connectionString);
            if (found == null) {
                using (var conn = new SqlConnection(connectionString)) {
                    var sqlCommand = new SqlCommand {
                        CommandText = commandText,
                        CommandType = commandType
                    };

                    if (commandType == CommandType.StoredProcedure) {
                        deriveParametersForProcedure(commandText, connectionString, conn, sqlCommand);
                    }
                    else {
                        deriveParametersForInlineCommand(sqlCommand);
                    }
                    if (SqlCommandCacheTimeout != SqlCommandCacheTimeout.IsNeverCached) {
                        cachedCommands.Add(new SqlCommandCacheObject(commandText, connectionString, sqlCommand));
                    }
                    sqlCommand.Connection = new SqlConnection(connectionString);
                    return sqlCommand;
                }
            }
            else {
                return createCommandFromCachedDefinedCommand(found);
            }
        }

        static SqlCommandCacheObject getFromCache(string commandText, string connectionString) {
            var key = commandText + connectionString;
            var found = cachedCommands.FirstOrDefault(o => o.Key == key);
            if (found != null &&
                found.ExpiresAt < DateTime.Now) {
                found = null;
            }
            return found;
        }

        private static void deriveParametersForProcedure(string commandText, string connectionString, SqlConnection conn, SqlCommand sqlCommand) {
            var temp = new SqlCommand {
                CommandText = commandText,
                Connection = conn,
                CommandType = CommandType.StoredProcedure
            };
            conn.Open();
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
                    TypeName = typeName == null ? null : typeName
                });
                if (p.SqlDbType == SqlDbType.Structured) {
                    addStructureFieldForParameter(sqlCommand, typeName, connectionString);
                }
            }
            conn.Close();
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
                            structureFields.Add(new Tuple<SqlCommand, string, string>(referenceCommand, typeName, value));
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

        private static SqlCommand createCommandFromCachedDefinedCommand(SqlCommandCacheObject found) {
            var sqlCommand = new SqlCommand { CommandText = found.CommandText, Connection = new SqlConnection(found.ConnectionString) };
            sqlCommand.CommandType = found.SqlCommand.CommandType;
            foreach (SqlParameter item in found.SqlCommand.Parameters) {
                sqlCommand.Parameters.Add(new SqlParameter {
                    Direction = item.Direction,
                    ParameterName = item.ParameterName,
                    SqlDbType = item.SqlDbType,
                    Size = item.Size,
                    Precision = item.Precision,
                    Scale = item.Scale,
                    SourceColumn = item.ParameterName.Replace("@", ""),
                    TypeName = item.TypeName
                });
            }
            return sqlCommand;
        }

        public static void RemoveFromCache(SqlCommand command) {
            var found = getFromCache(command.CommandText, command.Connection.ConnectionString);
            if (found != null) {
                cachedCommands.Remove(found);
            }
        }

        public static SqlCommand RederiveCommand(SqlCommand command) {
            RemoveFromCache(command);
            return command.CommandText.GetCommand(command.Connection.ConnectionString);
        }
    }
}
