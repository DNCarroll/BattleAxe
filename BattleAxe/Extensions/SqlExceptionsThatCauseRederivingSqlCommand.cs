using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleAxe {
    public class SqlExceptionsThatCauseRederivingSqlCommand {
        public static bool ReexecuteCommand(SqlException sqlException, ref SqlCommand command) {
            if (SqlExceptionsThatCauseRederivingSqlCommand.Values.Contains(sqlException.Number)) {
                command = CommandMethods.RederiveCommand(command);
                if (command != null) {
                    return true;
                }
            }
            return false;
        }

        public static List<int> Values { get; set; } = new List<int> { 201, 8144 };
    }
}
