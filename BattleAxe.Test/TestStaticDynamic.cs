using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleAxe;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Collections;

namespace BattleAxe.Test
{


    public enum MethodType

    {
        Get,
        Set
    }

    public class PropertyDefinition
    {
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
            }
        }

        private MethodType _MethodType;
        public MethodType MethodType
        {
            get { return _MethodType; }
            set
            {
                _MethodType = value;
            }
        }

        private bool _IsSerializable;
        public bool IsPublic
        {
            get { return _IsSerializable; }
            set
            {
                _IsSerializable = value;
            }
        }

    }

    //public class TestStaticDynamic : DynamicObject, IBattleAxe
    //{

    //    //house the statics cached
    //    static GetValue get = BattleAxe.Compiler.GetHelper.Value<TestStaticDynamic>();
    //    static SetValue<TestStaticDynamic> set = BattleAxe.Compiler.SetHelper.Value<TestStaticDynamic>();

    //    static List<PropertyDefinition> staticProperties;
    //    Dictionary<string, object> dictionary = new Dictionary<string, object>();
    //    public object this[string property]
    //    {
    //        get
    //        {
    //            setUpStatics();
    //            if (staticProperties.FirstOrDefault(s => s.Item1 == property) != null)
    //            {
    //                return get(this, property);
    //            }
    //            else
    //            {
    //                object result = null;
    //                this.dictionary.TryGetValue(property, out result);
    //                return result;
    //            }
    //        }
    //        set
    //        {
    //            setUpStatics();
    //            var sp = staticProperties.FirstOrDefault(s => s.Name == property);
    //            if (sp != null && sp.IsPublic)
    //            {
    //                set(this, property, value);
    //            }
    //            else if (sp == null)
    //            {
    //                dictionary[property] = value;
    //            }
    //        }
    //    }

    //    void setUpStatics()
    //    {
    //        if (staticProperties == null)
    //        {
    //            staticProperties = new List<PropertyDefinition>();
    //            var props = this.GetType().GetProperties();
    //            foreach (var item in props)
    //            {
    //                var setMethod = item.SetMethod;
    //                var getMethod = item.GetMethod;
    //                var dontIgnore = item.GetCustomAttribute(typeof(JsonIgnoreAttribute)) != null;                    
    //                staticProperties.Add(new PropertyDefinition { MethodType = MethodType.Get, Name = item.Name, IsPublic = getMethod.IsPublic  && dontIgnore });
    //                staticProperties.Add(new PropertyDefinition { MethodType = MethodType.Set, Name = item.Name, IsPublic = setMethod.IsPublic && dontIgnore });

    //            }
    //        }
    //    }

    //    // If you try to get a value of a property 
    //    // not defined in the class, this method is called.
    //    public override bool TryGetMember(
    //        System.Dynamic.GetMemberBinder binder, out object result)
    //    {
    //        // Converting the property name to lowercase
    //        // so that property names become case-insensitive.
    //        string name = binder.Name;

    //        // If the property name is found in a dictionary,
    //        // set the result parameter to the property value and return true.
    //        // Otherwise, return false.
    //        var ret = dictionary.TryGetValue(name, out result);
    //        if (!ret)
    //        {
    //            result = null;
    //        }
    //        return true;
    //    }

    //    // If you try to set a value of a property that is
    //    // not defined in the class, this method is called.
    //    public override bool TrySetMember(
    //        System.Dynamic.SetMemberBinder binder, object value)
    //    {
    //        // Converting the property name to lowercase
    //        // so that property names become case-insensitive.
    //        dictionary[binder.Name] = value;

    //        // You can always add a value to a dictionary,
    //        // so this method always returns true.
    //        return true;
    //    }

    //    /// <summary>
    //    /// If a property value is a delegate, invoke it
    //    /// </summary>     
    //    public override bool TryInvokeMember
    //       (InvokeMemberBinder binder, object[] args, out object result)
    //    {
    //        if (dictionary.ContainsKey(binder.Name)
    //                  && dictionary[binder.Name] is Delegate)
    //        {
    //            result = (dictionary[binder.Name] as Delegate).DynamicInvoke(args);
    //            return true;
    //        }
    //        else
    //        {
    //            return base.TryInvokeMember(binder, args, out result);
    //        }
    //    }


    //    /// <summary>
    //    /// Return all dynamic member names
    //    /// </summary>
    //    /// <returns>
    //    public override IEnumerable<string> GetDynamicMemberNames()
    //    {
    //        return dictionary.Keys;
    //        //var ret = new List<string>();
    //        //ret.AddRange(dictionary.Keys.ToArray());
    //        //var staticNames = this.SerializableProperties();
    //        //if (staticNames != null)
    //        //{
    //        //    ret.AddRange(staticNames);
    //        //}
    //        //return ret.ToArray();
    //    }

    //    public void Dispose()
    //    {
    //        foreach (KeyValuePair<string, object> item in dictionary)
    //        {
    //            dictionary[item.Key] = null;
    //        }
    //    }
    //}

    //public class CustomConverter : JsonConverter
    //{

    //    public override void WriteJson(JsonWriter writer,
    //                               object value,
    //                               JsonSerializer serializer)
    //    {
    //        if (value is TestStaticDynamic)
    //        {
    //            var ds = (TestStaticDynamic)value;
    //            string[] serializable;
    //            string[] notSerializable;
    //            ds.SetSerializableAndNotSerializable(out serializable, out notSerializable);
    //            var jobject = new JObject();
    //            foreach (var item in serializable)
    //            {
    //                var tempValue = ds[item];
    //                if (tempValue != null)
    //                {
    //                    jobject.Add(item, JToken.FromObject(tempValue));
    //                }
    //            }
    //            jobject.WriteTo(writer);
    //        }
    //        else
    //        {
    //            JToken t = JToken.FromObject(value);
    //            t.WriteTo(writer);
    //        }
    //    }

    //    object constructObject(Type type)
    //    {
    //        try
    //        {
    //            ConstructorInfo magicConstructor = type.GetConstructor(Type.EmptyTypes);
    //            return magicConstructor.Invoke(new object[] { });
    //        }
    //        catch (Exception)
    //        {
    //            throw;
    //        }

    //    }

    //    void hydrate(JObject jObject, TestStaticDynamic obj)
    //    {
    //        if (obj != null)
    //        {
    //            string[] serializable;
    //            string[] notSerializable;
    //            obj.SetSerializableAndNotSerializable(out serializable, out notSerializable);
    //            foreach (KeyValuePair<string, JToken> item in jObject)
    //            {
    //                try
    //                {
    //                    if (serializable.Contains(item.Key))
    //                    {
    //                        var type = obj.GetPropertyType(item.Key);
    //                        if (type != null)
    //                        {
    //                            if (type.IsValueType || type == typeof(string))
    //                            {
    //                                obj[item.Key] = item.Value;
    //                            }
    //                            else if (type.BaseType == typeof(TestStaticDynamic) && item.Value is JObject)
    //                            {
    //                                var childJObject = (JObject)item.Value;
    //                                if (obj[item.Key] == null)
    //                                {
    //                                    obj[item.Key] = constructObject(type);
    //                                }
    //                                hydrate(childJObject, (TestStaticDynamic)obj[item.Key]);
    //                            }
    //                            else if (item.Value is JObject)
    //                            {
    //                                JsonSerializer serializer = new JsonSerializer();
    //                                obj[item.Key] = serializer.Deserialize(new JTokenReader(item.Value), type);
    //                            }
    //                            else
    //                            {
    //                                obj[item.Key] = constructObject(type);
    //                                if (obj[item.Key] != null)
    //                                {
    //                                    if (obj[item.Key] is IList && item.Value is JArray && type.IsGenericType)
    //                                    {
    //                                        var jArray = ((JArray)item.Value).Children();
    //                                        var list = (IList)obj[item.Key];
    //                                        Type subType = list.GetType().GetGenericArguments()[0];
    //                                        var isDynamicSword = subType.BaseType == typeof(TestStaticDynamic);
    //                                        if (isDynamicSword)
    //                                        {
    //                                            foreach (var subItem in jArray)
    //                                            {
    //                                                if (subItem is JObject)
    //                                                {
    //                                                    var newSubObject = (TestStaticDynamic)constructObject(subType);
    //                                                    hydrate((JObject)subItem, newSubObject);
    //                                                    list.Add(newSubObject);
    //                                                }
    //                                            }
    //                                        }
    //                                        else
    //                                        {
    //                                            JsonSerializer serializer = new JsonSerializer();
    //                                            obj[item.Key] = serializer.Deserialize(new JTokenReader(item.Value), type);
    //                                        }
    //                                    }
    //                                }
    //                            }
    //                        }
    //                    }
    //                    else if (!notSerializable.Contains(item.Key))
    //                    {
    //                        obj[item.Key] = item.Value;
    //                    }
    //                }
    //                catch (Exception)
    //                {
    //                    throw;
    //                }
    //            }
    //        }
    //    }

    //    public override bool CanConvert(Type objectType)
    //    {
    //        return true;
    //    }

    //    public override object ReadJson(JsonReader reader,
    //                                    Type objectType,
    //                                     object existingValue,
    //                                     JsonSerializer serializer)
    //    {
    //        ConstructorInfo magicConstructor = objectType.GetConstructor(Type.EmptyTypes);
    //        var newObject = magicConstructor.Invoke(new object[] { });
    //        JObject jObject = JObject.Load(reader);
    //        if (newObject is TestStaticDynamic)
    //        {
    //            var ds = (TestStaticDynamic)newObject;
    //            hydrate(jObject, ds);
    //        }
    //        else
    //        {
    //            //do something different?
    //            //really shoulnt be in here anyways
    //        }
    //        return newObject;
    //    }

    //}
}
