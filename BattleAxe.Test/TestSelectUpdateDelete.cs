using System;
using System.Data;
using BattleAxe;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace BattleAxe.Test {

    
    [TestClass]
    public class TestSelectUpdateDelete {

        [TestMethod]
        public void CacheProcs() {
            var command = "testTable_Update".GetCommand(ConnectionString);
            var select = "testTable_Get".GetCommand(ConnectionString);
            var delete = "testTable_Delete".GetCommand(ConnectionString);
            var first = $"SELECT * FROM testTable WHERE TestID = @TestID".GetCommand(ConnectionString, 30, CommandType.Text);
            var deleteAll = "DELETE FROM TestTable".GetCommand(ConnectionString, 30, CommandType.Text);

            Assert.IsTrue(command != null && select != null && delete != null);
        }

        public string ConnectionString { get; set; } = "Boon0956-SF".WindowsAuthencationConnectionString("TestDatabase");
        [TestMethod]
        public void Test_1_TestInsert() {

            var newItem = new testTable { Name = "new thing", Note = "important" };
            "testTable_Update".Execute(ConnectionString, newItem);                       
            Assert.IsTrue(newItem.TestID > 0);
        }

        [TestMethod]
        public void Test_2_TestInsert2() {

            var newItem = new testTable { Name = "SomeOther", Note = "important" };
            (new ProcedureDefinition("testTable_Update", ConnectionString)).Execute(newItem);
            Assert.IsTrue(newItem.TestID > 0);
        }

        [TestMethod]
        public void Test_3_TestUpdate() {
            var items = "testTable_Get".ToList<testTable>(ConnectionString);
            var item = items.FirstOrDefault();
            item.Note = Guid.NewGuid().ToString();
            
            "testTable_Update".Execute(ConnectionString, item);
            var updateditem = $"SELECT * FROM testTable WHERE TestID = @TestID".FirstOrDefault<testTable>(ConnectionString, item, CommandType.Text);
            Assert.IsTrue(item != null && item.Note == updateditem.Note && item.Name == updateditem.Name);
        }

        [TestMethod]
        public void Test_4_TestDelete() {
            var item = "testTable_Get".FirstOrDefault<testTable>(ConnectionString);
            "testTable_Delete".Execute(ConnectionString, item);
            var found = $"SELECT * FROM testTable WHERE TestID = @TestID".FirstOrDefault<testTable>(ConnectionString, item, CommandType.Text);
            Assert.IsTrue(found == null);
        }

        [TestMethod]
        public void Test_6_RunABunch() {

            var oneHundred = get100();
            var commandDefinition = new ProcedureDefinition("testTable_Update", ConnectionString);
            var selectDefinition = new ProcedureDefinition("testTable_Get", ConnectionString);
            oneHundred.Update(commandDefinition);
            var items = selectDefinition.ToList<testTable>();
            Assert.IsTrue(items.Count > 99);

        }

        [TestMethod]
        public void Test_5_DeleteAll() {

            var textDefinition = new TextDefinition("DELETE FROM TestTable", ConnectionString);
            textDefinition.Execute();
            var selectDefinition = new ProcedureDefinition("testTable_Get", ConnectionString);
            var items = selectDefinition.ToList<testTable>();
            Assert.IsTrue(items.Count == 0);
        }

        [TestMethod]
        public void Test_7_Dynamic() {
            Dynamic dyn = new Dynamic();
            dyn["Name"] = "new dynamic";
            dyn["Note"] = "Note";
            dyn["TestNumber"] = 100;

            "testTable_Update".Execute(ConnectionString, dyn);
            Assert.IsTrue(dyn["TestID"] != null);
        }

        List<testTable> get100() {
            var ret = new List<testTable>();
            int i = 1;
            while (i < 101) {
                ret.Add(new testTable { Name = "Name" + i.ToString(), Note = "Note" + i.ToString(), TestNumber = i });
                i++;
            }
            return ret;
        }
    }

    public class testTable {

        public int TestID { get; set; }
        public int TestNumber { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public Guid GuidKey { get; set; } = Guid.NewGuid();

        private LookupKey lookupKey = LookupKey.Default;

        public LookupKey LookupKey {
            get {
                if (lookupKey == LookupKey.Default) {
                    var lastCharacter = TestNumber.ToString().LastOrDefault().ToString();
                    Enum.TryParse(lastCharacter, out lookupKey);
                }
                return lookupKey;
            }
            set { lookupKey = value; }
        }        
    }

    public enum LookupKey : byte {        
        Lookup0,
        Lookup1,
        Lookup2,
        Lookup3,
        Lookup4,
        Lookup5,
        Lookup6,
        Lookup7,
        Lookup8,
        Lookup9,
        Default = 100
    }
}
