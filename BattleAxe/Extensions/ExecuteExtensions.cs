using System;
using System.Data;
using System.Data.SqlClient;

namespace BattleAxe {
    public static class ExecuteExtensions {

        /// <summary>
        /// Inline command no parameters have been supplied to it in form of @ParameterName
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static int Execute(this string commandText, string connectionString) {
            int ret = -1;
            using (var connection = new SqlConnection(connectionString)) {
                using (var command = new SqlCommand(commandText, connection)) {
                    connection.Open();
                    ret = command.ExecuteNonQuery();
                }
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="definition"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static T Execute<T>(this ICommandDefinition definition, T parameter = null)
            where T : class {
            using (var connection = new SqlConnection(definition.ConnectionString)) {
                using (var command = definition.GetCommand()) {
                    command.Connection = connection;
                    connection.Open();
                    try {
                        ParameterMethods.SetInputs(parameter, command);
                        command.ExecuteNonQuery();
                        ParameterMethods.SetOutputs(parameter, command);
                    }
                    catch (SqlException sqlException) {
                        var deriveResult = SqlExceptionsThatCauseRederivingSqlCommand.ReexecuteCommand(sqlException, command);
                        if (deriveResult.Item1) {
                            return definition.Execute(parameter);
                        }
                        else {
                            throw sqlException;
                        }
                    }
                    catch {
                        throw;
                    }
                }
            }
            return parameter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static Dynamic Execute(this ICommandDefinition definition, Dynamic parameter = null) => definition.Execute<Dynamic>(parameter);

        /// <summary>
        /// The commandText will be derived here.  Form the command text up with @ParameterNames if using CommandType.Text and
        /// parameter values are being passed to the command from the T parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="connectionString"></param>
        /// <param name="parameter"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static T Execute<T>(this string commandText, string connectionString, T parameter = null, CommandType? commandType = null)
            where T : class => new CommandDefinition(commandText, connectionString, commandType).Execute(parameter);
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <param name="definition"></param>
        /// <returns></returns>
        public static T Execute<T>(this T parameter, ICommandDefinition definition) where T : class => definition.Execute(parameter);

    }
}