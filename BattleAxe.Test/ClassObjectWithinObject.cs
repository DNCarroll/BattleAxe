using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BattleAxe.Test {
    [TestClass]
    public class ClassObjectWithinObject {
        [TestMethod]
        public void TestMethod1() {
            var someobject = new SomeObject();
            someobject.ID = 1;
            var setMethod = Compiler.SetMethod(someobject);
            var getMethod = Compiler.GetMethod(someobject);
            setMethod(someobject, "ID", 3);
            var id = getMethod(someobject, "ID");
            Assert.IsTrue(id.ToString() == "3");
        }
    }
    public class SomeObject {
        public int ID { get; set; }
        public string Name { get; set; }

        public SomeObject2 SomeInternalObject { get; set; }
    }

    public class SomeObject2 {
        public int id { get; set; }
        public string name { get; set; }

    }
}
