using System;
using System.Collections.Generic;
using d = System.Data;

/// <summary>
/// BattleAxe.Blunt are extension methods that work against any class no Interfaces or special needs
/// </summary>
namespace BattleAxe.Blunt
{
    public static class Extensions
    {
        #region FirstOrDefault

        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>       
        public static T FirstOrDefault<T>(this d.SqlClient.SqlCommand command, T parameter = null)
            where T : class, new()
        {
            T newObj = null;
            try
            {
                if (command.IsConnectionOpen())
                {
                    setCommandParameters(parameter, command);
                    newObj = getFirstFromDataReader<T>(command);
                    setOutputParameters(parameter, command);                    
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                command.CloseConnection();
            }
            return newObj;
        }

        //this may not work with extension on other one it may grab it by default
        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static T FirstOrDefault<T>(this T obj, d.SqlClient.SqlCommand command)
            where T : class, new()
        {
            return FirstOrDefault<T>(command, obj);
        }

        private static T getFirstFromDataReader<T>(d.SqlClient.SqlCommand command) where T : class, new()
        {
            T newObj = new T();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    setValuesFromReader(newObj, reader);
                    break;
                }
            }

            return newObj;
        }
        #endregion

        #region ToList

        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this d.SqlClient.SqlCommand command, T parameter = null)
            where T : class, new()
        {
            List<T> ret = new List<T>();
            try
            {
                if (command.IsConnectionOpen())
                {
                    setCommandParameters(parameter, command);
                    using (var reader = command.ExecuteReader())
                    {
                        var map = DataReaderMap.GetReaderMap(reader);
                        while (reader.Read())
                        {
                            T newObj = new T();
                            DataReaderMap.Set(reader, map, newObj);
                            ret.Add(newObj);
                        }
                    }
                    setOutputParameters(parameter, command);
                }
            }
            catch
            {
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
        public static List<T> ToList<T>(this T obj, d.SqlClient.SqlCommand command)
            where T : class, new()
        {
            return ToList<T>(command, obj);
        }

        #endregion

        #region Execute

        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T Execute<T>(this d.SqlClient.SqlCommand command, T obj = null)
            where T : class
        {
            try
            {
                setCommandParameters(obj, command);
                if (command.IsConnectionOpen())
                {
                    command.ExecuteNonQuery();
                    setOutputParameters(obj, command);
                }
                command.Connection.Close();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                command.Connection.Close();
            }
            return obj;
        }

        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static T Execute<T>(this T obj, d.SqlClient.SqlCommand command)
            where T : class
        {
            return Execute(command, obj);
        }

        #endregion

        #region Update

        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. IBattleAxe assumes that the object is controlling
        /// all the value setting through the Indexer.
        /// beware this has no error trapping so make sure to trap your errors 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static List<T> Update<T>(this d.SqlClient.SqlCommand command, List<T> objs)
            where T: class
        {
            try
            {
                if (command.IsConnectionOpen())
                {
                    foreach (var obj in objs)
                    {
                        setCommandParameters(obj, command);
                        command.ExecuteNonQuery();
                        setOutputParameters(obj, command);
                    }
                }
                command.CloseConnection();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
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
        public static void Update<T>(this List<T> objs, d.SqlClient.SqlCommand command)
            where T : class
        {
            Update(command, objs);
        }

        #endregion
         
        internal static void setValuesFromReader<T>(T obj, d.IDataReader reader)
            where T : class
        {
            //reader.FieldCount
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var fieldName = reader.GetName(i);
                //this might be slow this is where some mapper would be handy
                //if considering larger results sets
                //which are mostly not existing                
                if (reader.GetFieldType(i) == typeof(byte[]))
                {
                    //reader all bytes
                    long size = reader.GetBytes(i, 0, null, 0, 0);
                    byte[] values = new byte[size];
                    int bufferSize = 1024;
                    long bytesRead = 0;
                    int curPos = 0;
                    while (bytesRead < size)
                    {
                        bytesRead += reader.GetBytes(i, curPos, values, curPos, bufferSize);
                        curPos += bufferSize;
                    }
                    obj.SetValue(fieldName, values);
                }
                else
                {
                    if (reader.IsDBNull(i))
                    {
                        obj.SetValue(fieldName, null);
                    }
                    else
                    {
                        var value = reader.GetValue(i);
                        obj.SetValue(fieldName, value);
                    }
                }
            }
        }

        internal static void setOutputParameters<T>(T obj, d.IDbCommand command)
            where T : class
        {
            if (obj != null)
            {
                try
                {
                    foreach (d.IDataParameter p in command.Parameters)
                    {
                        if (p.Direction == d.ParameterDirection.Output || p.Direction == d.ParameterDirection.InputOutput)
                        {
                            object value = p.Value;
                            if (value == DBNull.Value)
                            {
                                value = null;
                            }
                            obj.SetValue(!string.IsNullOrEmpty(p.SourceColumn) ? p.SourceColumn : p.ParameterName.Replace("@", ""), value);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        internal static void setCommandParameters<T>(T obj, d.IDbCommand command)
            where T : class
        {
            if (obj != null)
            {
                try
                {
                    foreach (d.IDbDataParameter parameter in command.Parameters)
                    {
                        if (parameter.Direction == d.ParameterDirection.Input ||
                            parameter.Direction == d.ParameterDirection.InputOutput)
                        {

                            var value = obj.GetValue(parameter.SourceColumn);
                            if (value != null)
                            {
                                parameter.Value = value;
                            }
                            else
                            {
                                parameter.Value = DBNull.Value;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public static d.SqlClient.SqlCommand GetCommand(this string commandText, string connectionString, d.CommandType commandType = d.CommandType.StoredProcedure)
        {
            return Common.GetCommand(commandText, connectionString, commandType);
        }
    }
}
