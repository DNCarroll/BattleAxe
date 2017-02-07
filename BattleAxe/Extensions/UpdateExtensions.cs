using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace BattleAxe {
    public static class UpdateExtensions {

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="definition"></param>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static List<T> Update<T>(this ICommandDefinition definition, List<T> objs)
            where T : class {
            using (var connection = new SqlConnection(definition.ConnectionString)) {
                using (var command = definition.GetCommand()) {
                    command.Connection = connection;
                    connection.Open();
                    try {
                        foreach (var obj in objs) {
                            ParameterMethods.SetInputs(obj, command);
                            command.ExecuteNonQuery();
                            ParameterMethods.SetOutputs(obj, command);
                        }
                    }
                    catch (SqlException sqlEx) when (sqlEx.ShouldTryReexecute()) {
                        if (command.IsCommandBuilt()) {
                            objs = definition.Update(objs);
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
            return objs;
        }

        public static List<Dynamic> Update(this ICommandDefinition definition, List<Dynamic> objs) => definition.Update<Dynamic>(objs);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="command"></param>
        public static void Update(this List<Dynamic> objs, ICommandDefinition command) => Update<Dynamic>(command, objs);


        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. IBattleAxe assumes that the object is controlling
        /// all the value setting through the Indexer.
        /// beware this has no error trapping so make sure to trap your errors 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objs"></param>
        /// <param name="where"></param>
        /// <param name="definition"></param>
        public static void Update<T>(this List<T> objs, Func<T, bool> where, ICommandDefinition definition)
            where T : class {
            var updates = objs.Where(where).ToList();
            definition.Update(objs);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objs"></param>
        /// <param name="commandText"></param>
        /// <param name="connectionString"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static List<T> Update<T>(this List<T> objs, string commandText, string connectionString, CommandType? commandType = null)
            where T : class => new CommandDefinition(commandText, connectionString, commandType).Update(objs);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objs"></param>
        /// <param name="definition"></param>
        /// <returns></returns>
        public static List<T> Update<T>(this List<T> objs, ICommandDefinition definition)
            where T : class => definition.Update(objs);

    }
}
