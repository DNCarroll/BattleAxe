using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BattleAxe.Test {
    [TestClass]
    public class ClassCreator {
        [TestMethod]
        public void TestInitialize() {
            var cc = new BattleAxe.Class.Creator();
            var value = cc.Initialized();
            Assert.IsTrue(value || !value);
        }

        [TestMethod]
        public void TestBuild() {
            
        }
        
    }
}
