using System.Data;
using System.Data.SqlClient;

namespace BattleAxe {
    public static class FirstOrDefaultExtensions {

        /// <summary>
        /// /
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="definition"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static T FirstOrDefault<T>(this ICommandDefinition definition, T parameter = null) where T : class, new() {
            T newObj = null;
            using (var connection = new SqlConnection(definition.ConnectionString)) {
                using (var command = definition.GetCommand()) {
                    command.Connection = connection;
                    connection.Open();
                    try {
                        ParameterMethods.SetInputs(parameter, command);
                        newObj = DataReaderMethods.GetFirst<T>(command);
                        ParameterMethods.SetOutputs(parameter, command);
                    }
                    catch (SqlException sqlEx) when (sqlEx.ShouldTryReexecute()) {
                        if (command.IsCommandBuilt()) {
                            newObj =  definition.FirstOrDefault(parameter);
                        }
                        else {
                            throw sqlEx;
                        }
                    }
                    catch {
                        throw;
                    }
                }
            }
            return newObj;
        }

        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. 
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="parameter"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static R FirstOrDefault<R, P>(this P parameter, ICommandDefinition definition)
            where R : class, new()
            where P : class {
            R newObj = null;
            using (var connection = new SqlConnection(definition.ConnectionString)) {
                using (var command = definition.GetCommand()) {
                    command.Connection = connection;
                    connection.Open();
                    try {
                        ParameterMethods.SetInputs(parameter, command);
                        newObj = DataReaderMethods.GetFirst<R>(command);
                        ParameterMethods.SetOutputs(parameter, command);
                    }
                    catch (SqlException sqlEx) when (sqlEx.ShouldTryReexecute()) {
                        if (command.IsCommandBuilt()) {
                            newObj = parameter.FirstOrDefault<R, P>(definition);
                        }
                        else {
                            throw sqlEx;
                        }
                    }
                    catch {
                        throw;
                    }
                }
            }
            return newObj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static Dynamic FirstOrDefault(this ICommandDefinition definition, BattleAxe.Dynamic parameter = null) =>
            definition.FirstOrDefault<Dynamic>(parameter);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="connectionString"></param>
        /// <param name="parameter"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static T FirstOrDefault<T>(this string commandText, string connectionString, T parameter = null, CommandType? commandType = null)
            where T : class, new() => new CommandDefinition(commandText, connectionString, commandType).FirstOrDefault(parameter);
        

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <param name="definition"></param>
        /// <returns></returns>
        public static T FirstOrDefault<T>(this T parameter, ICommandDefinition definition) where T: class, new() => definition.FirstOrDefault<T>(parameter);
        
    }
}
