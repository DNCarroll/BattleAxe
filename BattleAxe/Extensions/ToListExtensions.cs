using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BattleAxe {
    public static class ToListExtensions {

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="definition"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this ICommandDefinition definition, T parameter = null)
            where T : class, new() {
            List<T> ret = new List<T>();

            using (var connection = new SqlConnection(definition.ConnectionString)) {
                using (var command = definition.GetCommand()) {
                    command.Connection = connection;
                    connection.Open();
                    try {
                        ParameterMethods.SetInputs(parameter, command);
                        executeReaderAndFillList(command, ret);
                        ParameterMethods.SetOutputs(parameter, command);
                    }
                    catch (SqlException sqlException) {
                        var deriveResult = SqlExceptionsThatCauseRederivingSqlCommand.ReexecuteCommand(sqlException, command);
                        if (deriveResult.Item1) {
                            return definition.ToList(parameter);
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
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="ret"></typeparam>
        /// <typeparam name="par"></typeparam>
        /// <param name="definition"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static List<ret> ToList<ret, par>(this ICommandDefinition definition, par parameter = null)
            where ret : class, new()
            where par : class {

            List<ret> newList = new List<ret>();
            using (var connection = new SqlConnection(definition.ConnectionString)) {
                using (var command = definition.GetCommand()) {
                    command.Connection = connection;
                    connection.Open();
                    try {
                        if (command.IsConnectionOpen()) {
                            ParameterMethods.SetInputs(parameter, command);
                            executeReaderAndFillList(command, newList);
                            ParameterMethods.SetOutputs(parameter, command);
                        }
                    }
                    catch (SqlException sqlException) {
                        var deriveResult = SqlExceptionsThatCauseRederivingSqlCommand.ReexecuteCommand(sqlException, command);
                        if (deriveResult.Item1) {
                            return definition.ToList<ret, par>(parameter);
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
            return newList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static List<Dynamic> ToList(this ICommandDefinition definition, Dynamic parameter = null) => definition.ToList<Dynamic>(parameter);

      
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static List<Dynamic> ToList(this Dynamic parameter, ICommandDefinition definition) => ToList<Dynamic>(definition, parameter);
        
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="connectionString"></param>
        /// <param name="parameter"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this string commandText, string connectionString, T parameter = null, CommandType? commandType = null)
            where T : class, new() => new CommandDefinition(commandText, connectionString, commandType).ToList(parameter);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <param name="definition"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this T parameter, ICommandDefinition definition)
            where T : class, new() => definition.ToList(parameter);

    }
}