using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleAxe {
    public static class SqlExceptionsThatCauseRederivingSqlCommand {
        public static bool ShouldTryReexecute(this SqlException ex) {
            return SqlExceptionsThatCauseRederivingSqlCommand.Values.Contains(ex.Number);
        }

        public static bool IsCommandBuilt(this SqlCommand command) {
            var newCommand = CommandMethods.RederiveCommand(command);
            return newCommand != null;
        }

        public static List<int> Values { get; set; } = new List<int> { 201, 8144 };
    }
}
