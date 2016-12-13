using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleAxe.Test {
    [TestClass]
    public class ClassWithEnumTest {
        [TestMethod]
        public void TestMethod1() {
            var someobject = new ClassWithEnum();
            someobject.EnumTest = TestEnum.value1;
            var setMethod = Compiler.SetMethod(someobject);
            var getMethod = Compiler.GetMethod(someobject);
            
            setMethod(someobject, "EnumTest", TestEnum.value2);

            var id = getMethod(someobject, "EnumTest");
            Assert.IsTrue(id.ToString() == "value2");
        }
    }

    public class ClassWithEnum {
        public int ID { get; set; }
        public string Name { get; set; }
        public TestEnum EnumTest { get; set; }

    }

    public enum TestEnum {
        value1,
        value2
    }
}


namespace BattleAxe {
    public static class SetMethods {

        public static void SetValue(BattleAxe.Test.ClassWithEnum obj, string propertyName, object value) {
            switch (propertyName) {
                case "ID": obj.ID = value is int ? (int)value : value != null ? Convert.ToInt32(value) : 0; break;
                case "Name": obj.Name = value is string ? (string)value : value != null ? Convert.ToString(value) : System.String.Empty; break;
                case "EnumTest": obj.EnumTest = value == null ? default(BattleAxe.Test.TestEnum) : (BattleAxe.Test.TestEnum)System.Enum.Parse(typeof(BattleAxe.Test.TestEnum), value.ToString()); break;

                default:
                    break;
            }

        }

    }
}
