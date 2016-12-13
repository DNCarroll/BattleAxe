using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace BattleAxe {
    public static class UpdateExtensions {

        public static List<Dynamic> Update(this SqlCommand command, List<Dynamic> objs) => command.Update<Dynamic>(objs);
        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. IBattleAxe assumes that the object is controlling
        /// all the value setting through the Indexer.
        /// beware this has no error trapping so make sure to trap your errors 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static List<T> Update<T>(this SqlCommand command, List<T> objs)
            where T : class {
            try {
                if (command.IsConnectionOpen()) {
                    foreach (var obj in objs) {
                        ParameterMethods.SetInputs(obj, command);
                        command.ExecuteNonQuery();
                        ParameterMethods.SetOutputs(obj, command);
                    }
                }
            }
            catch (SqlException sqlException) {
                if (SqlExceptionsThatCauseRederivingSqlCommand.ReexecuteCommand(sqlException, ref command)) {
                    return command.Update(objs);
                }
                else {
                    throw sqlException;
                }
            }
            catch (Exception) {
                throw;
            }
            finally {
                command.CloseConnection();
            }
            return objs;
        }

        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. IBattleAxe assumes that the object is controlling
        /// all the value setting through the Indexer.
        /// beware this has no error trapping so make sure to trap your errors 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objs">the objects that you want to update with the provided command</param>
        /// <param name="command"></param>
        public static void Update<T>(this List<T> objs, SqlCommand command) where T : class => Update(command, objs);

        public static void Update(this List<Dynamic> objs, SqlCommand command) => Update<Dynamic>(command, objs);


        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. IBattleAxe assumes that the object is controlling
        /// all the value setting through the Indexer.
        /// beware this has no error trapping so make sure to trap your errors 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objs"></param>
        /// <param name="where"></param>
        /// <param name="command"></param>
        public static void Update<T>(this List<T> objs, Func<T, bool> where, SqlCommand command)
            where T : class {
            var updates = objs.Where(where).ToList();
            Update(command, objs);
        }
    }
}
