using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace BattleAxe.Test
{
    [TestClass]
    public class TestSomeInserts
    {

        [TestMethod]
        public void ForceASqlMissingParameterError()
        {
            var connString = "Data Source=CEARVALL;Initial Catalog=boosttraining;Integrated Security=True";
            var commandText = "AAAAADeleteWhenDoneTesting";
            var command = commandText.GetCommand(connString);
            //System.Threading.Thread.Sleep(30000);
            var testObject = new TestObjectForDatabase { FirstName = "Nathan", LastName = "Carroll", ID = 1 };
            try
            {
                testObject.Execute(command);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [TestMethod]
        public void ForceTimeoutOnCache()
        {
            var connString = "Data Source=CEARVALL;Initial Catalog=boosttraining;Integrated Security=True";
            var commandText = "AAAAADeleteWhenDoneTesting";
            //System.Threading.Thread.Sleep(30000);
            BattleAxe.CommandMethods.SqlCommandCacheTimeout = SqlCommandCacheTimeout.FifteenMinutes;
            var command = commandText.GetCommand(connString);

            var testObject = new TestObjectForDatabase { FirstName = "Nathan", LastName = "Carroll", ID = 1 };
            try
            {
                System.Threading.Thread.Sleep(new TimeSpan(0, 15, 20));
                command = commandText.GetCommand(connString);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [TestMethod]
        public void SetAndGetOfBattleAxeDynamic()
        {

            var objs = getSetAndGetData();
            Assert.IsTrue(passesTest(objs));

        }

        bool passesTest(List<BattleAxe.Dynamic> objs)
        {
            var _passedTest = true;
            foreach (var item in objs)
            {
                if ((int)item["ID"] != (int)item["Value"] ||
                    (string)item["Name"] != ("Name" + item["ID"].ToString()))
                {
                    _passedTest = false;
                    break;
                }
            }
            return _passedTest;
        }

        List<BattleAxe.Dynamic> getSetAndGetData() {
            List<BattleAxe.Dynamic> objs = new List<BattleAxe.Dynamic>();
            for (int i = 0; i < 100; i++)
            {
                var obj = new BattleAxe.Dynamic();
                obj["ID"] = i;
                obj["Value"] = i;
                obj["Name"] = "Name" + i.ToString();
                objs.Add(obj);
            }
            return objs;
        }
    }
}
