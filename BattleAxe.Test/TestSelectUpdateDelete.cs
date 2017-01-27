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
            "testTable_Update".Execute(ConnectionString, newItem);           
            var item = $"SELECT * FROM testTable WHERE id ={newItem.id}".FirstOrDefault<testTable>(ConnectionString);

            Dynamic dyn = new Dynamic();
            dyn["name"] = "new dynamic";
            dyn["value"] = "important";
            "testTable_Update".Execute(ConnectionString, dyn);


            Assert.IsTrue(item != null && item.value == newItem.value && item.name == newItem.name);
        }

        [TestMethod]
        public void TestUpdate() {
            var items = "testTable_Get".ToList<testTable>(ConnectionString);
            var item = items.FirstOrDefault();
            item.value = Guid.NewGuid().ToString();
            item.valueStatus = ValueStatus.Updated;
            "testTable_Update".Execute(ConnectionString, item);
            var updateditem = $"SELECT * FROM testTable WHERE id ={item.id}".FirstOrDefault<testTable>(ConnectionString);
            Assert.IsTrue(item != null && item.value == updateditem.value && item.name == updateditem.name);
        }

        [TestMethod]
        public void TestDelete() {
            var item = "testTable_Get".FirstOrDefault<testTable>(ConnectionString);
            "testTable_Delete".Execute(ConnectionString, item);
            var found = $"SELECT * FROM testTable WHERE id ={item.id}".FirstOrDefault<testTable>(ConnectionString);
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
