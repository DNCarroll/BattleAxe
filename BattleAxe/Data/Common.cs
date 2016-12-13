using System;
using System.Collections.Generic;
using System.Linq;
using d = System.Data;

namespace BattleAxe
{
    public static class Common
    {
        public static void Remove<S, T>(this List<S> source, List<T> itemsToEvaluteForRemoval, Func<T, S, bool> matchForRemoval)
        {
            foreach (var item in itemsToEvaluteForRemoval)
            {
                var found = source.FirstOrDefault(s => matchForRemoval(item, s));
                if (found != null)
                {
                    source.Remove(found);
                }
            }
        }

        public static void Remove<T>(this List<T> source, Func<T, bool> removeIfMatch)
        {
            int pos = source.Count - 1;
            while (pos > -1)
            {
                var item = source[pos];
                if (removeIfMatch(item))
                {
                    source.Remove(item);
                }
                pos--;
            }
        }

        public static void ActOn<S, T>(this List<S> source, List<T> itemsToEvaluate, Func<T, S, bool> whenMatched, Action<T, S> doThis)
        {
            foreach (var item in itemsToEvaluate)
            {
                var found = source.FirstOrDefault(s => whenMatched(item, s));
                if (found != null)
                {
                    doThis(item, found);
                }
            }
        }


        private static int m_Timeout = 30;
        /// <summary>
        /// Time in seconds to wait before timeout
        /// </summary>
        public static int Timeout
        {
            get { return m_Timeout; }
            set { m_Timeout = value; }
        }

        private static List<Tuple<string, string, d.SqlClient.SqlCommand>> m_cachedCommands = new List<Tuple<string, string, d.SqlClient.SqlCommand>>();
        internal static List<Tuple<string, string, d.SqlClient.SqlCommand>> cachedCommands
        {
            get { return m_cachedCommands; }
            set { m_cachedCommands = value; }
        }


        private static List<Tuple<d.SqlClient.SqlCommand, string, string>> m_StructureFields = new List<Tuple<d.SqlClient.SqlCommand, string, string>>();
        internal static List<Tuple<d.SqlClient.SqlCommand, string, string>> structureFields
        {
            get { return m_StructureFields; }
            set
            {
                m_StructureFields = value;
            }
        }

        /// <summary>
        /// execute a string back to the database using group names from regex to supply to the stored procedures
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="source">raw line that will be parsed via regex</param>
        /// <param name="regex"></param>
        /// <param name="regexNotFoundOrValueIsNull">if the parameter cannot find group this is method called for the value of the parameter</param>
        /// <returns></returns>
        public static T ExecuteWithRegex<T>(this d.SqlClient.SqlCommand command, string source, System.Text.RegularExpressions.Regex regex,
            Func<string, object> regexNotFoundOrValueIsNull = null)
            where T : class, IBattleAxe, new()
        {
            var outputParameters = new T();
            if (regex.TrySetSqlCommandParameterValues(source, command, regexNotFoundOrValueIsNull))
            {
                try
                {
                    if (command.IsConnectionOpen())
                    {
                        command.ExecuteNonQuery();
                        command.Connection.Close();
                        foreach (d.IDbDataParameter parameter in command.Parameters)
                        {
                            if (parameter.Direction == d.ParameterDirection.InputOutput || parameter.Direction == d.ParameterDirection.Output)
                            {
                                var field = parameter.ParameterName.Replace("@", "");
                                outputParameters[field] = parameter.Value;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string formatted = string.Format("Execution of 'ExecuteWithRegex' SqlCommand:{0}, ErrorMessage:{1}", command.CommandText, ex.Message);
                    throw new Exception(formatted);
                }
                finally {
                    command.Connection.Close();
                }
            }
            else
            {
                string formatted = string.Format("Failed to 'ExecuteWithRegex' SqlCommand:{0}", command.CommandText);
                throw new Exception(formatted);
            }
            return outputParameters;
        }

        public static bool TrySetSqlCommandParameterValues(this System.Text.RegularExpressions.Regex regex,
            string source,
            d.IDbCommand command,
            Func<string, object> regexNotFoundOrValueIsNull)
        {
            var match = regex.Match(source);
            if (match.Success)
            {
                foreach (d.SqlClient.SqlParameter item in command.Parameters)
                {
                    item.Value = DBNull.Value;
                    var group = match.Groups[item.SourceColumn];
                    if (group != null &&
                        !string.IsNullOrEmpty(group.Value))
                    {
                        item.Value = group.Value.Trim();
                    }
                    else if (regexNotFoundOrValueIsNull != null)
                    {
                        var value = regexNotFoundOrValueIsNull(item.SourceColumn);
                        if (value != null)
                        {
                            item.Value = value;
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public static string ConnectionStringTimeout(string connectionString)
        {
            if (connectionString.IndexOf("Connection Timeout") == -1 && Timeout != 15)
            {
                connectionString += ";Connection Timeout=" + Timeout.ToString();
            }
            return connectionString;
        }

        private static void addStructureFieldForParameter(d.SqlClient.SqlCommand referenceCommand, string typeName, string connectionString)
        {
            var commandString =
@"select 
    c.name FieldName
from
    sys.table_types tt inner
join
sys.columns c on c.object_id = tt.type_table_object_id
where
    USER_NAME(tt.schema_id) + '.' + tt.name = '" + typeName + "'";

            using (var conn = new d.SqlClient.SqlConnection(connectionString))
            {
                using (var command = new d.SqlClient.SqlCommand(commandString, conn))
                {
                    conn.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string value = reader.GetString(0);
                            structureFields.Add(new Tuple<d.SqlClient.SqlCommand, string, string>(referenceCommand, typeName, value));
                        }
                    }
                }
            }

        }

        /// <summary>
        /// this will derive the parameters if this is a StoredProcedure type of command
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="connectionString"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static d.SqlClient.SqlCommand GetCommand(string commandText, string connectionString, d.CommandType commandType = d.CommandType.StoredProcedure)
        {
            connectionString = ConnectionStringTimeout(connectionString);
            var found = cachedCommands.FirstOrDefault(o => o.Item1 == commandText && o.Item2 == connectionString);
            if (found == null)
            {
                using (var conn = new d.SqlClient.SqlConnection(connectionString))
                {
                    var sqlCommand = new d.SqlClient.SqlCommand
                    {
                        CommandText = commandText,
                        CommandType = commandType
                    };
                    if (commandText.Length > 6)
                    {
                        if (commandText.ToLower().Substring(0, 6) == "select")
                        {
                            commandType = d.CommandType.Text;
                        }
                    }

                    if (commandType == d.CommandType.StoredProcedure)
                    {

                        var temp = new System.Data.SqlClient.SqlCommand
                        {
                            CommandText = commandText,
                            Connection = conn,
                            CommandType = d.CommandType.StoredProcedure
                        };
                        conn.Open();
                        System.Data.SqlClient.SqlCommandBuilder.DeriveParameters(temp);
                        foreach (System.Data.SqlClient.SqlParameter p in temp.Parameters)
                        {
                            var typeName = p.TypeName;
                            if (typeName != null && typeName.Count(c => c == '.') == 2)
                            {
                                typeName = typeName.Substring(typeName.IndexOf(".") + 1);
                            }
                            sqlCommand.Parameters.Add(new d.SqlClient.SqlParameter
                            {
                                Direction = p.Direction,
                                ParameterName = p.ParameterName,
                                SqlDbType = p.SqlDbType,
                                Size = p.Size,
                                Precision = p.Precision,
                                Scale = p.Scale,
                                SourceColumn = p.ParameterName.Replace("@", ""),
                                TypeName = typeName == null ? null : typeName
                            });
                            if (p.SqlDbType == d.SqlDbType.Structured)
                            {
                                addStructureFieldForParameter(sqlCommand, typeName, connectionString);
                            }             
                        }
                        conn.Close();
                    }
                    else
                    {
                        sqlCommand.CommandType = d.CommandType.Text;
                        var regex = new System.Text.RegularExpressions.Regex("@\\w+");
                        var matches = regex.Matches(sqlCommand.CommandText);
                        foreach (System.Text.RegularExpressions.Match match in matches)
                        {
                            sqlCommand.Parameters.Add(new d.SqlClient.SqlParameter
                            {
                                ParameterName = match.Value,
                                SourceColumn = match.Value.Replace("@", "")
                            });
                        }
                    }
                    cachedCommands.Add(new Tuple<string, string, d.SqlClient.SqlCommand>(commandText, connectionString, sqlCommand));
                    sqlCommand.Connection = new d.SqlClient.SqlConnection(connectionString);
                    return sqlCommand;
                }
            }
            else
            {
                var sqlCommand = new d.SqlClient.SqlCommand { CommandText = found.Item1, Connection = new d.SqlClient.SqlConnection(found.Item2) };
                sqlCommand.CommandType = found.Item3.CommandType;
                foreach (d.SqlClient.SqlParameter item in found.Item3.Parameters)
                {
                    sqlCommand.Parameters.Add(new d.SqlClient.SqlParameter
                    {
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
        }
        
        public static bool IsConnectionOpen(this d.SqlClient.SqlCommand command)
        {
            var ret = false;
            if (command.Connection.State == System.Data.ConnectionState.Closed)
            {
                command.CommandTimeout = Common.Timeout;
                command.Connection.Open();
                ret = true;
            }
            else
            {
                ret = command.Connection.State == System.Data.ConnectionState.Open;
            }
            return ret;
        }

        public static void CloseConnection(this d.SqlClient.SqlCommand command)
        {
            if (command.Connection != null)
            {
                command.Connection.Close();
            }
        }

    }
}
