using System.Collections.Generic;
using System.Data.SqlClient;

namespace BattleAxe {
    public static class ToListExtensions {

        public static List<Dynamic> ToList(this SqlCommand command, Dynamic parameter = null) => command.ToList<Dynamic>(parameter);

        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this SqlCommand command, T parameter = null)
            where T : class, new() {
            List<T> ret = new List<T>();
            try {
                if (command.IsConnectionOpen()) {
                    ParameterMethods.SetInputs(parameter, command);
                    executeReaderAndFillList(command, ret);
                    ParameterMethods.SetOutputs(parameter, command);
                }
            }
            catch (SqlException sqlError) {
                if (SqlExceptionsThatCauseRederivingSqlCommand.ReexecuteCommand(sqlError, ref command)) {
                    return command.ToList(parameter);
                }
                else {
                    throw sqlError;
                }
            }
            catch {
                throw;
            }
            finally { command.CloseConnection(); }
            return ret;
        }

        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this T parameter, SqlCommand command)
            where T : class, new() => ToList<T>(command, parameter);

        public static List<Dynamic> ToList(this Dynamic parameter, SqlCommand command) => ToList<Dynamic>(command, parameter);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="ret"></typeparam>
        /// <typeparam name="par"></typeparam>
        /// <param name="command"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static List<ret> ToList<ret, par>(this SqlCommand command, par parameter = null)
            where ret : class, new()
            where par : class {

            List<ret> newList = new List<ret>();
            try {
                if (command.IsConnectionOpen()) {
                    ParameterMethods.SetInputs(parameter, command);
                    executeReaderAndFillList(command, newList);
                    ParameterMethods.SetOutputs(parameter, command);
                }
            }
            catch (SqlException sqlException) {
                if (SqlExceptionsThatCauseRederivingSqlCommand.ReexecuteCommand(sqlException, ref command)) {
                    return ToListExtensions.ToList<ret, par>(command, parameter);
                }
                else {
                    throw sqlException;
                }
            }
            catch {
                throw;
            }
            finally { command.CloseConnection(); }
            return newList;
        }

        private static void executeReaderAndFillList<T>(SqlCommand command, List<T> ret) where T : class, new() {
            var setMethod = Compiler.SetMethod(new T());
            using (var reader = command.ExecuteReader()) {
                var map = DataReaderMap.GetReaderMap(reader);
                while (reader.Read()) {
                    T newObj = new T();
                    DataReaderMap.Set(reader, map, newObj, setMethod);
                    ret.Add(newObj);
                }
            }
        }
    }
}