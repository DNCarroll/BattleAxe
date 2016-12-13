using System.Data.SqlClient;

namespace BattleAxe {
    public static class ConnectionMaintenance {
        /// <summary>
        /// Time in seconds to wait before timeout
        /// </summary>
        public static int Timeout { get; set; } = 30;

        public static bool IsConnectionOpen(this SqlCommand command) {
            var ret = false;
            if (command.Connection.State == System.Data.ConnectionState.Closed) {
                command.CommandTimeout = Timeout;
                command.Connection.Open();
                ret = true;
            }
            else {
                ret = command.Connection.State == System.Data.ConnectionState.Open;
            }
            return ret;
        }

        public static void CloseConnection(this SqlCommand command) {
            if (command.Connection != null) {
                command.Connection.Close();
            }
        }

        public static string ConnectionStringTimeout(string connectionString) {
            if (connectionString.IndexOf("Connection Timeout") == -1 && Timeout != 15) {
                connectionString += ";Connection Timeout=" + Timeout.ToString();
            }
            return connectionString;
        }

        public static string WindowsAuthencationConnectionString(this string server, string catalog) {
            return $"Data Source={server};Initial Catalog={catalog};Integrated Security=SSPI";
        }
    }
}
