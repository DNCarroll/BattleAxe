using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleAxe.Model
{
    public class TestObject
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public SomeOtherObject SomeotherObejct { get; set; }

        public BattleAxe.Model2.SomeOtherOtherObject SomeOther { get; set; }
    }

    public class SomeOtherObject {
        public string Value { get; set; }
    }
}
