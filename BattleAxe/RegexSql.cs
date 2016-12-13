using System;
using System.Data;
using System.Data.SqlClient;

namespace BattleAxe {
    public static class RegexSql {

        /// <summary>
        /// execute a string back to the database using group names from regex to supply to the stored procedures
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="source">raw line that will be parsed via regex</param>
        /// <param name="regex"></param>
        /// <param name="regexNotFoundOrValueIsNull">if the parameter cannot find group this is method called for the value of the parameter</param>
        /// <returns></returns>
        public static T ExecuteWithRegex<T>(this SqlCommand command, string source, System.Text.RegularExpressions.Regex regex,
            Func<string, object> regexNotFoundOrValueIsNull = null)
            where T : class, IBattleAxe, new() {
            var outputParameters = new T();
            if (regex.TrySetSqlCommandParameterValues(source, command, regexNotFoundOrValueIsNull)) {
                try {
                    if (command.IsConnectionOpen()) {
                        command.ExecuteNonQuery();
                        command.Connection.Close();
                        foreach (IDbDataParameter parameter in command.Parameters) {
                            if (parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Output) {
                                var field = parameter.ParameterName.Replace("@", "");
                                outputParameters[field] = parameter.Value;
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    throw new Exception($"Execution of 'ExecuteWithRegex' SqlCommand:{command.CommandText}, ErrorMessage:{ex.Message}");
                }
                finally {
                    command.Connection.Close();
                }
            }
            else {                
                throw new Exception($"Failed to 'ExecuteWithRegex' SqlCommand:{command.CommandText}");
            }
            return outputParameters;
        }

        public static bool TrySetSqlCommandParameterValues(this System.Text.RegularExpressions.Regex regex,
            string source,
            IDbCommand command,
            Func<string, object> regexNotFoundOrValueIsNull) {
            var match = regex.Match(source);
            if (match.Success) {
                foreach (SqlParameter item in command.Parameters) {
                    item.Value = DBNull.Value;
                    var group = match.Groups[item.SourceColumn];
                    if (group != null &&
                        !string.IsNullOrEmpty(group.Value)) {
                        item.Value = group.Value.Trim();
                    }
                    else if (regexNotFoundOrValueIsNull != null) {
                        var value = regexNotFoundOrValueIsNull(item.SourceColumn);
                        if (value != null) {
                            item.Value = value;
                        }
                    }
                }
                return true;
            }
            return false;
        }
    }
}
