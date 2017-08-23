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
        internal static List<SqlCommandCacheObject> CachedCommands { get; set; } = new List<SqlCommandCacheObject>();
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
            connectionString = ConnectionMaintenance.ConnectionStringTimeout(connectionString);
            var found = getFromCache(commandText, connectionString);
            if (found == null) {
                using (var conn = new SqlConnection(connectionString)) {
                    var sqlCommand = new SqlCommand {
                        CommandText = commandText,
                        CommandType = commandType,
                        CommandTimeout = commandTimeout
                    };

                    if (commandType == CommandType.StoredProcedure) {
                        CommandBuilder.DeriveParametersForProcedure(commandText, connectionString, sqlCommand);
                    }
                    else {
                        deriveParametersForInlineCommand(sqlCommand);
                    }
                    if (SqlCommandCacheTimeout != SqlCommandCacheTimeout.IsNeverCached) {
                        CachedCommands.Add(new SqlCommandCacheObject(commandText, connectionString, sqlCommand));
                    }
                    return sqlCommand;
                }
            }
            else {
                return createCommandFromCachedDefinedCommand(found);
            }
        }

        public static SqlCommand GetCommand(this ICommandDefinition definition) => 
            definition.CommandText.GetCommand(definition.ConnectionString, definition.CommandTimeout, definition.CommandType);

        static SqlCommandCacheObject getFromCache(string commandText, string connectionString) {
            var key = commandText + connectionString;
            var found = CachedCommands.FirstOrDefault(o => o.Key == key);
            if (found != null &&
                found.ExpiresAt < DateTime.Now) {
                found = null;
            }
            return found;
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
            var sqlCommand = new SqlCommand { CommandText = found.CommandText };            
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
                CachedCommands.Remove(found);
            }
        }

        public static SqlCommand RederiveCommand(SqlCommand command) {
            RemoveFromCache(command);
            return command.CommandText.GetCommand(command.Connection.ConnectionString, command.CommandTimeout);
        }
    }
}
