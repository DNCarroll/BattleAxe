using System;
using System.Data.SqlClient;

namespace BattleAxe
{
    public class SqlCommandCacheObject
    {
        public DateTime ExpiresAt { get; set; } = DateTime.MaxValue;
        public string Key { get; set; }
        public DateTime Initialized { get; set; } = new DateTime();
        public string ConnectionString { get; set; }
        public string CommandText { get; set; }
        public SqlCommand SqlCommand { get; set; }

        public SqlCommandCacheObject(string commandText, string connectionString,  SqlCommand sqlCommand)
        {
            this.CommandText = commandText;
            this.ConnectionString = connectionString;
            this.SqlCommand = sqlCommand;
            this.Key = commandText + connectionString;
            if (CommandMethods.SqlCommandCacheTimeout != SqlCommandCacheTimeout.NeverExpires)
            {
                this.ExpiresAt = DateTime.Now.AddMinutes((int)CommandMethods.SqlCommandCacheTimeout);
            }
        }
    }
}
