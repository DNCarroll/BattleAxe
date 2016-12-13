using System;
using System.Collections.Generic;
using d = System.Data;
using System.Linq;
using System.Collections;

/// <summary>
/// BattleAxe.Sharp are extension methods that work against IBattleAxe
/// </summary>
namespace BattleAxe.Sharp
{
    public static class Extensions
    {
        #region FirstOrDefault

        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. IBattleAxe assumes that the object is controlling
        /// all the value setting through the Indexer
        /// beware this has no error trapping so make sure to trap your errors
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static T FirstOrDefault<T>(this d.SqlClient.SqlCommand command, T parameter = null)
            where T : class, IBattleAxe, new()
        {
            T newObj = null;
            try
            {
                if (command.IsConnectionOpen())
                {
                    setCommandParameterValues(parameter, command);
                    newObj = getFirstFromReader<T>(command);
                    setOutputParameters(parameter, command);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                command.CloseConnection();
            }
            return newObj;
        }

        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. IBattleAxe assumes that the object is controlling
        /// all the value setting through the Indexer
        /// beware this has no error trapping so make sure to trap your errors
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static T FirstOrDefault<T>(this d.SqlClient.SqlCommand command, IBattleAxe parameter)
            where T : class, IBattleAxe, new()
        {
            T newObj = null;
            try
            {
                if (command.IsConnectionOpen())
                {
                    setCommandParameterValues(parameter, command);
                    newObj = getFirstFromReader<T>(command);
                    setOutputParameters(parameter, command);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                command.CloseConnection();
            }
            return newObj;
        }

        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. 
        /// beware this has no error trapping so make sure to trap your errors
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static T FirstOrDefault<T>(this T obj, d.SqlClient.SqlCommand command)
            where T : class, IBattleAxe, new()
        {
            return command.FirstOrDefault<T>(obj);
        }

        private static T getFirstFromReader<T>(d.SqlClient.SqlCommand command) where T : class, IBattleAxe, new()
        {
            T newObj = null;
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    newObj = new T();
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
        /// the string should be set. IBattleAxe assumes that the object is controlling
        /// all the value setting through the Indexer
        /// beware this has no error trapping so make sure to trap your errors
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this d.SqlClient.SqlCommand command, T parameter)
            where T : class, IBattleAxe, new()
        {
            List<T> ret = new List<T>();
            try
            {
                if (command.IsConnectionOpen())
                {
                    setCommandParameterValues(parameter, command);
                    executeReaderAndFillList(command, ret);
                    setOutputParameters(parameter, command);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                command.CloseConnection();
            }
            return ret;
        }

        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. IBattleAxe assumes that the object is controlling
        /// all the value setting through the Indexer
        /// beware this has no error trapping so make sure to trap your errors
        /// </summary>
        /// <typeparam name="T">the return type</typeparam>
        /// <param name="command"></param>
        /// <param name="parameter">IBattle axe so can find parameter values from it using indexer</param>
        /// <returns></returns>
        public static List<T> ToList<T>(this d.SqlClient.SqlCommand command, IBattleAxe parameter)
            where T : class, IBattleAxe, new()
        {
            List<T> ret = new List<T>();
            try
            {
                if (command.IsConnectionOpen())
                {
                    setCommandParameterValues(parameter, command);
                    executeReaderAndFillList(command, ret);
                    setOutputParameters(parameter, command);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                command.CloseConnection();
            }
            return ret;
        }

        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. 
        /// beware this has no error trapping so make sure to trap your errors
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this d.SqlClient.SqlCommand command)
            where T : class, IBattleAxe, new()
        {
            List<T> ret = new List<T>();
            try
            {
                if (command.IsConnectionOpen())
                {
                    executeReaderAndFillList(command, ret);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                command.CloseConnection();
            }
            return ret;
        }

        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. 
        /// beware this has no error trapping so make sure to trap your errors
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this T obj, d.SqlClient.SqlCommand command)
            where T : class, IBattleAxe, new()
        {
            return command.ToList<T>(obj);
        }

        private static void executeReaderAndFillList<T>(d.SqlClient.SqlCommand command, List<T> ret) where T : class, IBattleAxe, new()
        {
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
        }

        #endregion

        #region Execute

        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. IBattleAxe assumes that the object is controlling
        /// all the value setting through the Indexer. 
        /// beware this has no error trapping so make sure to trap your errors
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static T Execute<T>(this T obj, d.SqlClient.SqlCommand command)
            where T : class, IBattleAxe, new()
        {
            return command.Execute(obj);
        }

        /// <summary>
        /// the command should have the connections string set,  doesnt have to be open but
        /// the string should be set. IBattleAxe assumes that the object is controlling
        /// all the value setting through the Indexer.
        /// beware this has no error trapping so make sure to trap your errors 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T Execute<T>(this d.SqlClient.SqlCommand command, T obj)
            where T : class, IBattleAxe, new()
        {
            try
            {                
                if (command.IsConnectionOpen())
                {
                    setCommandParameterValues(obj, command);
                    command.ExecuteNonQuery();
                    setOutputParameters(obj, command);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                command.CloseConnection();
            }
            return obj;
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
            where T : class, IBattleAxe   
        {
            try
            {             
                if (command.IsConnectionOpen())
                {
                    foreach (var obj in objs)
                    {
                        setCommandParameterValues(obj, command);
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
            where T : class, IBattleAxe
        {
            command.Update(objs);
        }

        #endregion

        #region Helper Methods

        internal static void setValuesFromReader<T>(T obj, d.IDataReader reader)
            where T : IBattleAxe
        {
            //reader.FieldCount
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var fieldName = reader.GetName(i);
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
                    obj[fieldName] = values;
                }
                else
                {
                    var value = reader.GetValue(i);
                    if (reader.IsDBNull(i))
                    {
                        obj[fieldName] = null;
                    }
                    else
                    {
                        obj[fieldName] = value;
                    }
                }
            }
        }

        internal static void setOutputParameters<T>(T obj, d.SqlClient.SqlCommand command)
            where T : IBattleAxe
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
                        obj[!string.IsNullOrEmpty(p.SourceColumn) ? p.SourceColumn : p.ParameterName.Replace("@", "")] = value;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        internal static void setCommandParameterValues<T>(T obj, d.SqlClient.SqlCommand command, bool shipStructured = false)
            where T : IBattleAxe
        {
            if (obj != null)
            {
                try
                {
                    foreach (d.SqlClient.SqlParameter parameter in command.Parameters)
                    {
                        if (parameter.SqlDbType != d.SqlDbType.Structured)
                        {
                            if (parameter.Direction == d.ParameterDirection.Input ||
                                parameter.Direction == d.ParameterDirection.InputOutput)
                            {
                                var value = obj[parameter.SourceColumn];
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
                        else if (!shipStructured)
                        {
                            parameter.Value = GetDataTable(obj[parameter.SourceColumn], command, parameter);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public static void SetStructuredParameterValue<T>(this d.SqlClient.SqlCommand command, string parameterName, List<T> data)
        {
            var parameter = command.Parameters[parameterName];
            if (parameter != null)
            {
                d.DataTable ret = new System.Data.DataTable();
                var reference = Common.structureFields.Where(i => i.Item1.CommandText == command.CommandText && i.Item1.Connection.ConnectionString == command.Connection.ConnectionString && i.Item2 == parameter.TypeName).ToList();
                foreach (var item in reference)
                {
                    ret.Columns.Add(item.Item3);
                }
                foreach (var obj in data)
                {
                    IBattleAxe ibattleAxe = (IBattleAxe)obj;
                    d.DataRow row = ret.NewRow();
                    foreach (var item in reference)
                    {
                        var value = ibattleAxe[item.Item3];
                        if (value is Enum)
                        {
                            row[item.Item3] = (int)value;
                        }
                        else
                        {
                            row[item.Item3] = value;
                        }
                    }
                    ret.Rows.Add(row);
                }
                parameter.Value = ret;
            }
        }

        public static void SetSimpleParameterValues<T>(this d.SqlClient.SqlCommand command, T obj)
            where T : IBattleAxe
        {
            setCommandParameterValues(obj, command, true);
        }
        internal static d.DataTable GetDataTable(object referenceObject, d.SqlClient.SqlCommand command, d.SqlClient.SqlParameter parameter)
        {
            d.DataTable ret = new System.Data.DataTable();
            if (referenceObject != null)
            {
                var reference = Common.structureFields.Where(i => i.Item1.CommandText == command.CommandText && i.Item1.Connection.ConnectionString == command.Connection.ConnectionString && i.Item2 == parameter.TypeName).ToList();
                foreach (var item in reference)
                {
                    ret.Columns.Add(item.Item3);
                }
                if (referenceObject is IBattleAxe)
                {
                    var ibattleAxe = (IBattleAxe)referenceObject;
                    d.DataRow row = ret.NewRow();
                    foreach (var item in reference)
                    {
                        row[item.Item3] = ibattleAxe[item.Item3];
                    }
                    ret.Rows.Add(row);
                }
                else
                {
                    var type = referenceObject.GetType();
                    if (type.Name == "List`1")
                    {
                        IList data = (IList)referenceObject;
                        foreach (var obj in data)
                        {
                            IBattleAxe ibattleAxe = (IBattleAxe)obj;
                            d.DataRow row = ret.NewRow();
                            foreach (var item in reference)
                            {
                                var value = ibattleAxe[item.Item3];
                                if (value is Enum)
                                {
                                    row[item.Item3] = (int)value;
                                }
                                else
                                {
                                    row[item.Item3] = value;
                                }
                            }
                            ret.Rows.Add(row);
                        }
                    }
                }
            }
            return ret;
        }

        #endregion

        public static d.SqlClient.SqlCommand GetCommand(this string commandText, string connectionString, d.CommandType commandType = d.CommandType.StoredProcedure)
        {
            return Common.GetCommand(commandText, connectionString, commandType);
        }
    }
}
