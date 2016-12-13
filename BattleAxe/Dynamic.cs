using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

namespace BattleAxe {
    public class Dynamic : DynamicObject, IBattleAxe, IDisposable {
        Dictionary<string, object> dictionary = new Dictionary<string, object>();
        public object this[string property] {
            get {
                object result = null;
                this.dictionary.TryGetValue(property, out result);
                return result;
            }
            set {
                dictionary[property] = value;
            }
        }

        // If you try to get a value of a property 
        // not defined in the class, this method is called.
        public override bool TryGetMember(
            System.Dynamic.GetMemberBinder binder, out object result) {
            // Converting the property name to lowercase
            // so that property names become case-insensitive.
            string name = binder.Name;

            // If the property name is found in a dictionary,
            // set the result parameter to the property value and return true.
            // Otherwise, return false.
            var ret = dictionary.TryGetValue(name, out result);
            if (!ret) {
                result = null;
            }
            return true;
        }

        // If you try to set a value of a property that is
        // not defined in the class, this method is called.
        public override bool TrySetMember(
            System.Dynamic.SetMemberBinder binder, object value) {
            // Converting the property name to lowercase
            // so that property names become case-insensitive.
            dictionary[binder.Name] = value;

            // You can always add a value to a dictionary,
            // so this method always returns true.
            return true;
        }

        /// <summary>
        /// If a property value is a delegate, invoke it
        /// </summary>     
        public override bool TryInvokeMember
           (InvokeMemberBinder binder, object[] args, out object result) {
            if (dictionary.ContainsKey(binder.Name)
                      && dictionary[binder.Name] is Delegate) {
                result = (dictionary[binder.Name] as Delegate).DynamicInvoke(args);
                return true;
            }
            else {
                return base.TryInvokeMember(binder, args, out result);
            }
        }


        /// <summary>
        /// Return all dynamic member names
        /// </summary>
        /// <returns>
        public override IEnumerable<string> GetDynamicMemberNames() {
            return dictionary.Keys;
            //var ret = new List<string>();
            //ret.AddRange(dictionary.Keys.ToArray());
            //var staticNames = this.SerializableProperties();
            //if (staticNames != null)
            //{
            //    ret.AddRange(staticNames);
            //}
            //return ret.ToArray();
        }

        public void Dispose() {
            foreach (KeyValuePair<string, object> item in dictionary) {
                dictionary[item.Key] = null;
            }
        }
    }
}
