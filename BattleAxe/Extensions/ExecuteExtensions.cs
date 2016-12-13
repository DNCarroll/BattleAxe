using System;
using System.Data.SqlClient;

namespace BattleAxe {
    public static class ExecuteExtensions {

        public static Dynamic Execute(this SqlCommand command, Dynamic parameter) => command.Execute<Dynamic>(parameter);
        public static void Execute(this SqlCommand command) => command.Execute(new object());

        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. 
        /// </summary>s
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static T Execute<T>(this T parameter, SqlCommand command) where T : class => Execute(command, parameter);

        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T Execute<T>(this SqlCommand command, T parameter = null)
            where T : class {
            try {
                ParameterMethods.SetInputs(parameter, command);
                if (command.IsConnectionOpen()) {
                    command.ExecuteNonQuery();
                    ParameterMethods.SetOutputs(parameter, command);
                }
            }
            catch (SqlException sqlException) {
                if (SqlExceptionsThatCauseRederivingSqlCommand.ReexecuteCommand(sqlException, ref command)) {
                    return command.Execute(parameter);
                }
                else {
                    throw sqlException;
                }
            }
            catch (Exception) {
                throw;
            }
            finally {
                command.Connection.Close();
            }
            return parameter;
        }
    }
}