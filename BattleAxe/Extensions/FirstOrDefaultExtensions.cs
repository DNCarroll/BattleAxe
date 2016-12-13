using System.Data.SqlClient;

namespace BattleAxe {
    public static class FirstOrDefaultExtensions {

        public static BattleAxe.Dynamic FirstOrDefault(this SqlCommand command, BattleAxe.Dynamic parameter = null) => 
            command.FirstOrDefault<Dynamic>(parameter);


        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>       
        public static T FirstOrDefault<T>(this SqlCommand command, T parameter = null)
            where T : class, new() {
            T newObj = null;
            try {
                if (command.IsConnectionOpen()) {
                    ParameterMethods.SetInputs(parameter, command);
                    newObj = DataReaderMethods.GetFirst<T>(command);
                    ParameterMethods.SetOutputs(parameter, command);
                }
            }
            catch (SqlException sqlException) {
                if (SqlExceptionsThatCauseRederivingSqlCommand.ReexecuteCommand(sqlException, ref command)) {
                    return command.FirstOrDefault(parameter);
                }
                else {
                    throw sqlException;
                }
            }
            catch {
                throw;
            }
            finally {
                command.CloseConnection();
            }
            return newObj;
        }

        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static T FirstOrDefault<T>(this T parameter, SqlCommand command)
            where T : class, new() => command.FirstOrDefault(parameter);


        public static Dynamic FirstOrDefault(this Dynamic parameter, SqlCommand command) => 
            command.FirstOrDefault<Dynamic>(parameter);

        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. 
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="parameter"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static R FirstOrDefault<R, P>(this P parameter, SqlCommand command)
            where R : class, new()
            where P : class {
            R newObj = null;
            try {
                if (command.IsConnectionOpen()) {
                    ParameterMethods.SetInputs(parameter, command);
                    newObj = DataReaderMethods.GetFirst<R>(command);
                    ParameterMethods.SetOutputs(parameter, command);
                }
            }
            catch (SqlException sqlException) {
                if (SqlExceptionsThatCauseRederivingSqlCommand.ReexecuteCommand(sqlException, ref command)) {
                    return FirstOrDefaultExtensions.FirstOrDefault<R, P>(parameter, command);
                }
                else {
                    throw sqlException;
                }
            }
            catch {
                throw;
            }
            finally {
                command.CloseConnection();
            }
            return newObj;
        }
    }
}
