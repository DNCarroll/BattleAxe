using System;
using BattleAxe;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace BattleAxe.Test {
    [TestClass]
    public class TestSelectUpdateDelete {

        public string ConnectionString { get; set; } = "Data Source=CEARVALL;Initial Catalog=testDatabase;Integrated Security=True";
        [TestMethod]
        public void TestInsert() {
            var newItem = new testTable { name = "new thing", value = "important" };
            "testTable_Update".GetCommand(ConnectionString).Execute(newItem);
            var item = $"SELECT * FROM testTable WHERE id ={newItem.id}".GetCommand(ConnectionString, System.Data.CommandType.Text).FirstOrDefault<testTable>();
            Assert.IsTrue(item != null && item.value == newItem.value && item.name == newItem.name);
        }

        [TestMethod]
        public void TestUpdate() {
            var items = "testTable_Get".GetCommand(ConnectionString).ToList<testTable>();
            var item = items.FirstOrDefault();
            item.value = Guid.NewGuid().ToString();
            item.valueStatus = ValueStatus.Updated;
            "testTable_Update".GetCommand(ConnectionString).Execute(item);
            var updateditem = $"SELECT * FROM testTable WHERE id ={item.id}".GetCommand(ConnectionString, System.Data.CommandType.Text).FirstOrDefault<testTable>();
            Assert.IsTrue(item != null && item.value == updateditem.value && item.name == updateditem.name);
        }

        [TestMethod]
        public void TestDelete() {
            var item = "testTable_Get".GetCommand(ConnectionString).FirstOrDefault<testTable>();
            "testTable_Delete".GetCommand(ConnectionString).Execute(item);
            var found = $"SELECT * FROM testTable WHERE id ={item.id}".GetCommand(ConnectionString, System.Data.CommandType.Text).FirstOrDefault<testTable>();
            Assert.IsTrue(found == null);
        }
    }

    public class testTable {
        public int id { get; set; }
        public string value { get; set; }
        public string name { get; set; }
        public ValueStatus valueStatus { get; set; } = ValueStatus.New;

        //void asdf() {
        //   // "obj.{propertyName} = ({type})System.Enum.Parse(typeof({type}), value.ToString()); break;";
        //    object value = null;
        //    obj.{propertyName} = value == null ? default({type}) : ({type})System.Enum.Parse(typeof({type}), value.ToString()); break;
        //}
    }
}
