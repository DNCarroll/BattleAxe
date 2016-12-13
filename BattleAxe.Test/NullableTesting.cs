using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BattleAxe;

namespace BattleAxe.Test {
    [TestClass]
    public class TestSettingGettingOfNullableTypes {

        [TestMethod]
        public void TestInt() {            
            Assert.IsTrue(TestSetOfValue("NullableInt", 4));
        }


        [TestMethod]
        public void TestIntForNull() {
            var obj = new NullableTestClass();
            obj.NullableInt = 4;
            Assert.IsTrue(TestSetOfValue("NullableInt", null));
        }

        [TestMethod]
        public void TestBoolean() {
            object b = true;            
            Assert.IsTrue(TestSetOfValue("NullableBoolean", b));
        }

        [TestMethod]
        public void TestBooleanForNull() {
            var obj = new NullableTestClass();
            obj.NullableBoolean = true;
            Assert.IsTrue(TestSetOfValue("NullableBoolean", null));
        }


        [TestMethod]
        public void TestDouble() {
            Assert.IsTrue(TestSetOfValue("NullableDouble", 1));
        }

        [TestMethod]
        public void TestDoubleForNull() {
            var obj = new NullableTestClass();
            obj.NullableDouble = 1;
            Assert.IsTrue(TestSetOfValue("NullableDouble", null));
        }

        [TestMethod]
        public void TestAllFields() {
            var dict = new System.Collections.Generic.Dictionary<string, object>();
            dict.Add("NullableInt", 1);
            dict.Add("NullableBoolean", true);
            dict.Add("NullableDouble", (double)10);
            dict.Add("NullableByte", (byte)1);
            dict.Add("NullableShort", (short)1);
            dict.Add("NullableLong", (long)1);
            dict.Add("NullableSingle", (Single)1);
            dict.Add("NullableDecimal", (byte)1);
            dict.Add("NullableChar", (char)';');
            dict.Add("NullableGuid", Guid.NewGuid());
            dict.Add("NullableDateTime", DateTime.Now);
            bool passed = true;
            var obj = new NullableTestClass();
            foreach (System.Collections.Generic.KeyValuePair<string, object> item in dict) {
                if (!TestSetOfValue(item.Key, item.Value, obj)) {
                    passed = false;
                }
            }
            Assert.IsTrue(passed);
        }

        [TestMethod]
        public void TestAllFieldsGoBackToNull() {
            var dict = new System.Collections.Generic.Dictionary<string, object>();
            dict.Add("NullableInt", 1);
            dict.Add("NullableBoolean", true);
            dict.Add("NullableDouble", (double)10);
            dict.Add("NullableByte", (byte)1);
            dict.Add("NullableShort", (short)1);
            dict.Add("NullableLong", (long)1);
            dict.Add("NullableSingle", (Single)1);
            dict.Add("NullableDecimal", (byte)1);
            dict.Add("NullableChar", (char)';');
            dict.Add("NullableGuid", Guid.NewGuid());
            dict.Add("NullableDateTime", DateTime.Now);
            bool passed = true;
            var obj = new NullableTestClass();
            foreach (System.Collections.Generic.KeyValuePair<string, object> item in dict) {
                TestSetOfValue(item.Key, item.Value, obj);
                if(!TestSetOfValue(item.Key, null, obj)) {
                    passed = false;
                }
            }
            Assert.IsTrue(passed);
        }

        public static bool TestSetOfValue(string property, object value, NullableTestClass obj = null) {
            obj = obj ?? new NullableTestClass();
            var setMethod = Compiler.SetMethod(obj);
            var getMethod = Compiler.GetMethod(obj);
            setMethod(obj, property, value);
            var currentValue = getMethod(obj, property);
            if(currentValue == null && value == null) {
                return true;
            }
            return currentValue.ToString() == value.ToString();
        }
    }

    public class NullableTestClass {        

        public int? NullableInt { get; set; }
        public bool? NullableBoolean { get; set; }
        public double? NullableDouble { get; set; }
        public byte? NullableByte { get; set; }
        public short? NullableShort { get; set; }
        public long? NullableLong { get; set; }
        public Single? NullableSingle { get; set; }
        public decimal? NullableDecimal { get; set; }
        public char? NullableChar { get; set; }
        public Guid? NullableGuid { get; set; }
        public DateTime? NullableDateTime { get; set; }

    }
}
