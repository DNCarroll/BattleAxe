using System.Collections.Generic;
using System;

namespace BattleAxe {
    public class DataReaderMap {
        public string FieldName { get; set; }
        public System.Data.SqlDbType SqlType { get; set; }
        public int Index { get; set; }

        public static List<DataReaderMap> GetReaderMap(System.Data.IDataReader reader) {
            var ret = new List<DataReaderMap>();
            for (int i = 0; i < reader.FieldCount; i++) {
                var newMap = new DataReaderMap(i, reader);
                ret.Add(newMap);
            }
            return ret;
        }

        public DataReaderMap(int index, System.Data.IDataReader reader) {
            this.Index = index;
            this.FieldName = reader.GetName(this.Index);
            this.SqlType = sqlType(reader);
        }

        public static void Set<T>(System.Data.IDataReader reader, List<DataReaderMap> map, T obj, Action<T, string, object> setMethod)
            where T : class {
            foreach (var item in map) {
                item.SetFromReader(obj, reader, setMethod);
            }
        }

        public void SetFromReader<T>(T obj, System.Data.IDataReader reader, Action<T, string, object> setMethod)
            where T : class {
            if (!reader.IsDBNull(this.Index)) {
                switch (this.SqlType) {
                    case System.Data.SqlDbType.BigInt:
                        setMethod(obj, FieldName, reader.GetInt64(this.Index));
                        break;
                    case System.Data.SqlDbType.Image:
                    case System.Data.SqlDbType.VarBinary:
                    case System.Data.SqlDbType.Binary:
                        long size = reader.GetBytes(this.Index, 0, null, 0, 0);
                        byte[] values = new byte[size];
                        int bufferSize = 1024;
                        long bytesRead = 0;
                        int curPos = 0;
                        while (bytesRead < size) {
                            bytesRead += reader.GetBytes(this.Index, curPos, values, curPos, bufferSize);
                            curPos += bufferSize;
                        }
                        setMethod(obj, FieldName, values);
                        break;
                    case System.Data.SqlDbType.Bit:
                        setMethod(obj, FieldName, reader.GetBoolean(this.Index));
                        break;
                    case System.Data.SqlDbType.Char:
                        setMethod(obj, FieldName, reader.GetChar(this.Index));
                        break;
                    case System.Data.SqlDbType.SmallDateTime:
                    case System.Data.SqlDbType.Date:
                    case System.Data.SqlDbType.DateTime:
                    case System.Data.SqlDbType.DateTime2:
                        setMethod(obj, FieldName, reader.GetDateTime(this.Index));
                        break;
                    case System.Data.SqlDbType.SmallMoney:
                    case System.Data.SqlDbType.Money:
                    case System.Data.SqlDbType.Decimal:
                        setMethod(obj, FieldName, reader.GetDecimal(this.Index));
                        break;
                    case System.Data.SqlDbType.Float:
                        setMethod(obj, FieldName, reader.GetDouble(this.Index));
                        break;
                    case System.Data.SqlDbType.Int:
                        setMethod(obj, FieldName, reader.GetInt32(this.Index));
                        break;
                    case System.Data.SqlDbType.Text:
                    case System.Data.SqlDbType.NVarChar:
                    case System.Data.SqlDbType.NText:
                    case System.Data.SqlDbType.VarChar:
                    case System.Data.SqlDbType.NChar:
                        setMethod(obj, FieldName, reader.GetString(this.Index));
                        break;
                    case System.Data.SqlDbType.Real:
                        setMethod(obj, FieldName, reader.GetFloat(this.Index));
                        break;
                    case System.Data.SqlDbType.SmallInt:
                        setMethod(obj, FieldName, reader.GetInt16(this.Index));
                        break;
                    case System.Data.SqlDbType.TinyInt:
                        setMethod(obj, FieldName, reader.GetByte(this.Index));
                        break;
                    case System.Data.SqlDbType.UniqueIdentifier:
                        setMethod(obj, FieldName, reader.GetGuid(this.Index));
                        break;
                    default:
                        break;
                }
            }
            else {
                setMethod(obj, FieldName, null);
            }
        }

        System.Data.SqlDbType sqlType(System.Data.IDataReader reader) {
            var name = reader.GetFieldType(this.Index).Name;
            switch (name) {
                case "Int32": return System.Data.SqlDbType.Int;
                case "DateTime2":
                case "SmallDateTime":
                case "DateTime": return System.Data.SqlDbType.DateTime;
                case "String":
                case "Xml": return System.Data.SqlDbType.VarChar;
                case "Boolean": return System.Data.SqlDbType.Bit;
                case "Byte": return System.Data.SqlDbType.TinyInt;
                case "Double": return System.Data.SqlDbType.Float;
                case "Int16": return System.Data.SqlDbType.SmallInt;
                case "Int64": return System.Data.SqlDbType.BigInt;
                case "FileStream":
                case "byte[]": return System.Data.SqlDbType.Binary;
                case "Guid": return System.Data.SqlDbType.UniqueIdentifier;
                case "Money":
                case "Decimal": return System.Data.SqlDbType.Decimal;
                case "Single": return System.Data.SqlDbType.Real;
                default:
                    return System.Data.SqlDbType.Variant;
            }
        }
    }
}
