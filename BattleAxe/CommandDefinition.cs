using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BattleAxe {
    public interface ICommandDefinition {
        string ConnectionString { get; set; }
        string CommandText { get; set; }
        CommandType CommandType { get; set; }
    }

    public class ProcedureDefinition : CommandDefinition {
        public ProcedureDefinition(string commandText, string connectionString)
            : base(commandText, connectionString, CommandType.StoredProcedure) {
            if (commandText.IndexOf(".") == 0) {
                setCommandText();
            }
        }
    }

    public class CommandDefinition : ICommandDefinition {
        private string connectionString;
        public string ConnectionString {
            get {
                if (string.IsNullOrEmpty(connectionString)) {
                    throw new System.Exception("Command defintion requires connection string.");
                }
                return connectionString; }
            set { connectionString = value; }
        }

        private string commandText;

        public string CommandText {
            get {
                if (string.IsNullOrEmpty(commandText)) {
                    throw new System.Exception("Command definition requires command text.");
                }
                return commandText; }
            set { commandText = value; }
        }

        public CommandType CommandType { get; set; }

        /// <summary>
        /// If you supply the command text in form of schemaName.procedureName the command type will
        /// be set to StoredProcedure
        /// If you do not set the command type the process will look at sys.objects and 
        /// try to find the command text on the server database.  If it finds multiple schemas it wont know which 
        /// one to pick and will throw an exception.  If it doesnt find anything it will assume the 
        /// command text is Text based command.  If you make an inline sql statement with @ParameterName
        /// within the text the command parameters will be derived.
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="connectionString"></param>
        /// <param name="commandType"></param>
        public CommandDefinition(string commandText, string connectionString, CommandType? commandType = null) {
            this.commandText = commandText;
            this.connectionString = connectionString;            
            setCommandType(commandType);
        }

        void setCommandType(CommandType? commandType) {
            if (commandType.HasValue) {
                var pattern = "^\\w+\\.";
                if (System.Text.RegularExpressions.Regex.IsMatch(this.CommandText, pattern)) {
                    this.CommandType = System.Data.CommandType.StoredProcedure;
                }
                else {
                    setCommandText();
                    this.CommandType = System.Data.CommandType.Text;
                }
            }
            else {
                this.CommandType = commandType.Value;
            }
        }

        protected void setCommandText() {
            var existsOnServer = getSchemaAndProcedure();
            if (existsOnServer.Count == 1) {
                this.CommandText = $"{existsOnServer[0].Item1}.{this.CommandText}";
                this.CommandType = System.Data.CommandType.StoredProcedure;
            }
            else if (existsOnServer.Count > 0) {
                throw new Exception($"Procedure {this.CommandText} could not be determined their are multiple procedures with same name across the schemas.");
            }
        }


        protected List<Tuple<string, string>> getSchemaAndProcedure() {
            List<Tuple<string, string>> ret = new List<Tuple<string, string>>();
            using (var connection = new SqlConnection(connectionString)) {
                using (var command = new SqlCommand(commandText, connection)) {
                    connection.Open();
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            ret.Add(new Tuple<string, string>(reader.GetString(0), reader.GetString(1)));
                        }
                    }
                }
            }
            return ret;
        }

        string selectForProcedure(string commandText) {
            return $@"select
                        s.name schemaName,
                        o.name procedureName
                      from
                        sys.objects o inner join
                        sys.schemas s on o.schema_id = s.schema_id
                      where
                        s.name != 'sys' and
                        o.is_ms_shipped = 0 and
                        o.name = '{commandText}'
                        type not in ('P')";
        }

    }    

}
