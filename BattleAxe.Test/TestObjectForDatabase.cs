using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleAxe.Test
{
    public enum ValueStatus {
        New = 0,
        Updated = 1
    }

    public class TestObjectForDatabase
    {



        private int _ID;
        public int ID
        {
            get { return _ID; }
            set
            {
                _ID = value;
            }
        }

        private string _FirstName;
        public string FirstName
        {
            get { return _FirstName; }
            set
            {
                _FirstName = value;
            }
        }

        private string _LastName;
        public string LastName
        {
            get { return _LastName; }
            set
            {
                _LastName = value;
            }
        }
    }
}
