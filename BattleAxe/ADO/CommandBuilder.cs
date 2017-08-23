using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleAxe {
    public static class CommandBuilder {
        public static void DeriveParametersForProcedure(string commandText, string connectionString, SqlCommand sqlCommand) {
            using (var connection = new SqlConnection(connectionString)) {
                using (var temp = new SqlCommand(commandText, connection) { CommandType = CommandType.StoredProcedure }) {
                    connection.Open();
                    SqlCommandBuilder.DeriveParameters(temp);
                    foreach (SqlParameter p in temp.Parameters) {
                        var typeName = p.TypeName;
                        if (typeName != null && typeName.Count(c => c == '.') == 2) {
                            typeName = typeName.Substring(typeName.IndexOf(".") + 1);
                        }
                        sqlCommand.Parameters.Add(new SqlParameter {
                            Direction = p.Direction,
                            ParameterName = p.ParameterName,
                            SqlDbType = p.SqlDbType,
                            Size = p.Size,
                            Precision = p.Precision,
                            Scale = p.Scale,
                            SourceColumn = p.ParameterName.Replace("@", ""),
                            TypeName = typeName ?? null
                        });
                        if (p.SqlDbType == SqlDbType.Structured) {
                            addStructureFieldForParameter(sqlCommand, typeName, connectionString);
                        }
                    }
                }
            }
        }

        private static void addStructureFieldForParameter(SqlCommand referenceCommand, string typeName, string connectionString) {
            var commandString =
$@"select 
    c.name FieldName
from
    sys.table_types tt inner
join
sys.columns c on c.object_id = tt.type_table_object_id
where
    USER_NAME(tt.schema_id) + '.' + tt.name = '{ typeName }'";

            using (var conn = new SqlConnection(connectionString)) {
                using (var command = new SqlCommand(commandString, conn)) {
                    conn.Open();
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            string value = reader.GetString(0);
                            CommandMethods.StructureFields.Add(new Tuple<SqlCommand, string, string>(referenceCommand, typeName, value));
                        }
                    }
                }
            }

        }
    }
}
