using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BattleAxe {
    public delegate object GetValue(object obj, string propertyName);
    public delegate void SetValue<T>(T obj, string propertyName, object value)
        where T : class;

    public static class Compiler {
        static Dictionary<Type, object> setMethods = new Dictionary<Type, object>();
        static Dictionary<Type, object> getMethods = new Dictionary<Type, object>();

        public static Action<T, string, object> SetMethod<T>(T obj)
            where T : class {
            Action<T, string, object> setMethod = null;
            if (obj is IBattleAxe) {
                setMethod = (o, property, value) => ((IBattleAxe)o)[property] = value;                
            }
            else {
                var tempGetSetMethod = getSetMethodFromInitialReflection(obj);
                setMethod = (o, property, value) => tempGetSetMethod(o, property, value);                
            }
            return setMethod;
        }

        private static SetValue<T> getSetMethodFromInitialReflection<T>(T obj)
            where T : class {
            var type = typeof(T);
            if (setMethods.ContainsKey(type)) {
                var found = (SetValue<T>)setMethods[typeof(T)];
                return found;
            }
            else {
                var built = SetHelper.Value<T>(type);
                if (built != null) {
                    setMethods.Add(typeof(T), built);
                    return built;
                }
            }
            return null;
        }

        public static Func<T, string, object> GetMethod<T>(T sourceForInputParameters) where T : class {


            Func<T, string, object> getMethod;
            if (sourceForInputParameters is IBattleAxe) {
                getMethod = (obj, property) => {
                    IBattleAxe temp = (IBattleAxe)obj;
                    return temp[property];
                };
            }
            else {
                GetValue tempGetMethod = getGetMethodFromInitialReflection(sourceForInputParameters);
                getMethod = (obj, property) => tempGetMethod(obj, property);                
            }
            return getMethod;
        }

        public static GetValue GetMethod2(object obj) {
            var type = obj.GetType();
            var fullName = type.FullName;
            var assembly = type.Assembly;
            var types = assembly.GetTypes();
            var objectType = type.Assembly.GetTypes().FirstOrDefault(t => t.FullName == type.FullName);
            if (getMethods.ContainsKey(objectType)) {
                var found = (GetValue)getMethods[objectType];
                return found;
            }
            else {
                var built = GetHelper.Value(objectType);
                if (built != null) {
                    getMethods.Add(objectType, built);
                    return built;
                }
            }
            return null;
        }

        //load type form the FullName?
        private static GetValue getGetMethodFromInitialReflection<T>(T obj)
            where T : class {
            var type = typeof(T);
            if (getMethods.ContainsKey(type)) {
                var found = (GetValue)getMethods[typeof(T)];
                return found;
            }
            else {
                var built = GetHelper.Value(type);
                if (built != null) {
                    getMethods.Add(typeof(T), built);
                    return built;
                }
            }
            return null;
        }

        public static class SetHelper {
            static string caseStatement(string propertyName, string type, string convertMethod, string defaultValue = null) {

                var ret = $"case \"{propertyName}\": obj.{propertyName} = value is {type} ? ({type})value : " +
                    (defaultValue != null ?
                        $"value != null ? {convertMethod} : {defaultValue};" :
                        $"{convertMethod};"
                    ) + "break;";                
                return ret;
            }
            

            public static SetValue<T> Value<T>() where T : class => Value<T>(typeof(T));

            public static SetValue<T> Value<T>(Type type)
                where T : class {
                var classSet = getClass();
                var method = GetSetMethod(type);
                // methods.AppendLine(method);
                if (method != null) {
                    classSet = classSet.Replace("{method}", method);
                    //ready to build it
                    MethodInfo function = createMethod(classSet, type);
                    var betterFunction = (SetValue<T>)Delegate.CreateDelegate(typeof(SetValue<T>), function);
                    return betterFunction;
                }
                return null;
            }

            static MethodInfo createMethod(string code, Type type) {
                CSharpCodeProvider provider = new CSharpCodeProvider();
                CompilerParameters parameters = new CompilerParameters();

                // Reference to System.Drawing library
                //parameters.ReferencedAssemblies.Add("System.Drawing.dll");
                // True - memory generation, false - external file generation
                parameters.GenerateInMemory = true;
                parameters.GenerateExecutable = false;
                // Add CSharpSimpleScripting.exe as a reference to Scripts.dll to expose interfaces
                List<Assembly> referencedAssemblies = new List<Assembly>();
                referencedAssemblies.Add(type.Assembly);
                //parameters.ReferencedAssemblies.Add(type.Assembly.Location);
                var interfaces = type.GetInterfaces();
                foreach (var i in interfaces) {
                    var found = referencedAssemblies.FirstOrDefault(a => a == i.Assembly);
                    if (found == null) {
                        referencedAssemblies.Add(i.Assembly);
                    }
                }
                foreach (var a in referencedAssemblies) {
                    parameters.ReferencedAssemblies.Add(a.Location);
                }
                var location = new BattleAxe.Dynamic().GetType().Assembly.Location;
                //added so that we can use NullableConverter,  there were no references to BattleAxe
                parameters.ReferencedAssemblies.Add(location);
                CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

                Type binaryFunction = results.CompiledAssembly.GetType("BattleAxe.SetMethods");
                return binaryFunction.GetMethod("SetValue");
            }

            static string getClass() {

                string ret = @"
            using System;
            namespace BattleAxe
            {
                public static class SetMethods 
                {
                    {method}
                }
            }";
                return ret;
            }

            static string getSetValue(string type) {

                string ret = @"
                    public static void SetValue({type} obj, string propertyName, object value)
                    {
                        switch (propertyName)
                        {
                            {caseStatements}
                            default:
                                break;
                        }
        	
                    }
         ";
                ret = ret.Replace("{type}", type);
                return ret;
            }

            public static string GetSetMethod(Type type) {
                var properties = type.GetProperties();
                var cases = new StringBuilder();

                var method = getSetValue(type.FullName);

                foreach (var property in properties) {
                    //only add sets that can
                    var setmethod = property.SetMethod;
                    if (setmethod != null && setmethod.IsPublic && property.Name != "Item") {
                        var propertyName = property.Name;
                        var propertyType = property.PropertyType;
                        if (propertyType.Equals(typeof(int))) {
                            cases.AppendLine(caseStatement(propertyName, "int", "Convert.ToInt32(value)", "0"));
                        }
                        else if (propertyType.Equals(typeof(string))) {
                            cases.AppendLine(caseStatement(propertyName, "string", "Convert.ToString(value)", "System.String.Empty"));
                        }
                        else if (propertyType.Equals(typeof(bool))) {
                            cases.AppendLine(caseStatement(propertyName, "bool", "Convert.ToBoolean(value)", "false"));
                        }
                        else if (propertyType.Equals(typeof(double))) {
                            cases.AppendLine(caseStatement(propertyName, "double", "Convert.ToDouble(value)", "0"));
                        }
                        else if (propertyType.Equals(typeof(byte))) {
                            cases.AppendLine(caseStatement(propertyName, "byte", "Convert.ToByte(value)", "(byte)0"));
                        }
                        else if (propertyType.Equals(typeof(short))) {
                            cases.AppendLine(caseStatement(propertyName, "short", "Convert.ToInt16(value)", "(short)0"));
                        }
                        else if (propertyType.Equals(typeof(long))) {
                            cases.AppendLine(caseStatement(propertyName, "long", "Convert.ToInt64(value)", "0"));
                        }
                        else if (propertyType.Equals(typeof(Single))) {
                            cases.AppendLine(caseStatement(propertyName, "Single", "Convert.ToSingle(value)", "0"));
                        }
                        else if (propertyType.Equals(typeof(decimal))) {
                            cases.AppendLine(caseStatement(propertyName, "decimal", "Convert.ToDecimal(value)", "0"));
                        }
                        else if (propertyType.Equals(typeof(char))) {
                            cases.AppendLine(caseStatement(propertyName, "char", "Convert.ToChar(value)", "System.Char.MinValue"));
                        }
                        else if (propertyType.Equals(typeof(Guid))) {
                            cases.AppendLine(caseStatement(propertyName, "Guid", "new System.Guid(value.ToString())", "System.Guid.Empty"));
                        }
                        else if (propertyType.Equals(typeof(DateTime))) {
                            cases.AppendLine(caseStatement(propertyName, "DateTime", "Convert.ToDateTime(value)", "System.DateTime.MinValue"));
                        }
                        else if (propertyType.Equals(typeof(int?))) {
                            cases.AppendLine(caseStatement(propertyName, "int", "BattleAxe.NullConverter.ToInt(value)"));
                        }
                        else if (propertyType.Equals(typeof(bool?))) {
                            cases.AppendLine(caseStatement(propertyName, "bool", "BattleAxe.NullConverter.ToBoolean(value)"));
                        }
                        else if (propertyType.Equals(typeof(double?))) {
                            cases.AppendLine(caseStatement(propertyName, "double", "BattleAxe.NullConverter.ToDouble(value)"));
                        }
                        else if (propertyType.Equals(typeof(byte?))) {
                            cases.AppendLine(caseStatement(propertyName, "byte", "BattleAxe.NullConverter.ToByte(value)"));
                        }
                        else if (propertyType.Equals(typeof(short?))) {
                            cases.AppendLine(caseStatement(propertyName, "short", "BattleAxe.NullConverter.ToShort(value)"));
                        }
                        else if (propertyType.Equals(typeof(long?))) {
                            cases.AppendLine(caseStatement(propertyName, "long", "BattleAxe.NullConverter.ToLong(value)"));
                        }
                        else if (propertyType.Equals(typeof(Single?))) {
                            cases.AppendLine(caseStatement(propertyName, "Single", "BattleAxe.NullConverter.ToSingle(value)"));
                        }
                        else if (propertyType.Equals(typeof(decimal?))) {
                            cases.AppendLine(caseStatement(propertyName, "decimal", "BattleAxe.NullConverter.ToDecimal(value)"));
                        }
                        else if (propertyType.Equals(typeof(char?))) {
                            cases.AppendLine(caseStatement(propertyName, "char", "BattleAxe.NullConverter.ToChar(value)"));
                        }
                        else if (propertyType.Equals(typeof(Guid?))) {
                            cases.AppendLine(caseStatement(propertyName, "Guid", "BattleAxe.NullConverter.ToGuid(value)"));
                        }
                        else if (propertyType.Equals(typeof(DateTime?))) {
                            cases.AppendLine(caseStatement(propertyName, "DateTime", "BattleAxe.NullConverter.ToDateTime(value)"));
                        }
                        else if (propertyType.IsEnum) {
                            var typeName = propertyType.FullName;
                            var caseStatement = $"case \"{propertyName}\": obj.{propertyName} = value == null ? default({typeName}) : ({typeName})System.Enum.Parse(typeof({typeName}), value.ToString()); break;";                            
                            cases.AppendLine(caseStatement);
                        }
                        else if (propertyType.Equals(typeof(byte[]))) {
                            //dont really have an answer for this one yet
                            //we go through reader and it reads bytes                            
                            var caseStatement = $"case \"{propertyName}\": obj.{propertyName} = value!=null && value is byte[] ? (byte[])value: null; break;";                            
                            cases.AppendLine(caseStatement);
                        }
                        else if (!propertyType.IsClass &&
                                  propertyType.Equals(typeof(object))) {
                            var caseStatement = $"case \"{propertyName}\": obj.{propertyName} = ({type})value; break;";
                            cases.AppendLine(caseStatement);
                        }
                    }
                }
                if (cases.Length > 0) {
                    method = method.Replace("{caseStatements}", cases.ToString());
                    return method;
                }
                return null;
            }

            

        }

        public static class GetHelper {
            public static GetValue Value<T>()
                where T : class {
                return Value(typeof(T));
            }
            public static GetValue Value(Type type) {
                var classSet = getClass();
                var method = GetGetMethod(type);
                // methods.AppendLine(method);
                if (method != null) {
                    classSet = classSet.Replace("{method}", method);
                    //ready to build it
                    MethodInfo function = createMethod(classSet, type);
                    var betterFunction = (GetValue)Delegate.CreateDelegate(typeof(GetValue), function);
                    return betterFunction;
                }
                return null;
            }

            public static Func<object, string, string> Value2(Type type) {
                var classSet = getClass();
                var method = GetGetMethod(type);
                // methods.AppendLine(method);
                if (method != null) {
                    classSet = classSet.Replace("{method}", method);
                    //ready to build it
                    MethodInfo function = createMethod(classSet, type);
                    var betterFunction = (Func<object, string, string>)Delegate.CreateDelegate(typeof(Func<object, string, string>), function);
                    return betterFunction;
                }
                return null;
            }

            static string getClass() {

                string ret = @"
            using System;
            namespace BattleAxe
            {
                public static class GetMethods 
                {
                    {method}
                }
            }";
                return ret;
            }

            public static string GetGetMethod(Type type) {
                var properties = type.GetProperties();
                var cases = new StringBuilder();

                var method = getGetValue(type.FullName);
                //only add properties that have get 
                //Item = the property index?
                foreach (var property in properties) {
                    var getmethod = property.GetMethod;
                    if (getmethod != null && getmethod.IsPublic && property.Name != "Item") {
                        var propertyName = property.Name;
                        var propertyType = property.PropertyType;
                        if (propertyType.Equals(typeof(DateTime))) {
                            cases.AppendLine($"case \"{propertyName}\": return obj.{propertyName} == System.DateTime.MinValue ? null : (object)obj.{propertyName};");
                        }
                        else if(!propertyType.IsClass || propertyType.Equals(typeof(string))) {
                            cases.AppendLine($"case \"{propertyName}\": return obj.{propertyName};");
                        }
                    }
                }
                if (cases.Length > 0) {
                    method = method.Replace("{caseStatements}", cases.ToString());
                    return method;
                }
                return null;
            }

            static string getGetValue(string type) {

                string ret = @"
                    public static object GetValue(object justAnObject, string propertyName)
                    {
                        if(justAnObject is {type})
                        {
                            {type} obj = ({type})justAnObject;
                            switch (propertyName)
                            {
                                {caseStatements}
                                default:
                                    return null;
                            }
                        }
        	            return null;
                    }
         ";
                ret = ret.Replace("{type}", type);
                return ret;
            }

            static MethodInfo createMethod(string code, Type type) {
                CSharpCodeProvider provider = new CSharpCodeProvider();
                CompilerParameters parameters = new CompilerParameters();

                // Reference to System.Drawing library
                //parameters.ReferencedAssemblies.Add("System.Drawing.dll");
                // True - memory generation, false - external file generation
                parameters.GenerateInMemory = true;
                parameters.GenerateExecutable = false;
                // Add CSharpSimpleScripting.exe as a reference to Scripts.dll to expose interfaces
                List<Assembly> referencedAssemblies = new List<Assembly>();
                referencedAssemblies.Add(type.Assembly);
                //parameters.ReferencedAssemblies.Add(type.Assembly.Location);
                var interfaces = type.GetInterfaces();
                foreach (var i in interfaces) {
                    var found = referencedAssemblies.FirstOrDefault(a => a == i.Assembly);
                    if (found == null) {
                        referencedAssemblies.Add(i.Assembly);
                    }
                }
                foreach (var a in referencedAssemblies) {
                    parameters.ReferencedAssemblies.Add(a.Location);
                }

                CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

                Type binaryFunction = results.CompiledAssembly.GetType("BattleAxe.GetMethods");
                return binaryFunction.GetMethod("GetValue");
            }
        }

        
    }

    public static class NullConverter {
        public static int? ToInt(object value) {
            if (value != null) {
                return System.Convert.ToInt32(value);
            }
            return null;
        }
        public static bool? ToBoolean(object value) {
            if (value != null) {
                return System.Convert.ToBoolean(value);
            }
            return null;
        }
        public static double? ToDouble(object value) {
            if (value != null) {
                return System.Convert.ToDouble(value);
            }
            return null;
        }
        public static byte? ToByte(object value) {
            if (value != null) {
                return System.Convert.ToByte(value);
            }
            return null;
        }
        public static short? ToShort(object value) {
            if (value != null) {
                return System.Convert.ToInt16(value);
            }
            return null;
        }
        public static long? ToLong(object value) {
            if (value != null) {
                return System.Convert.ToInt64(value);
            }
            return null;
        }
        public static Single? ToSingle(object value) {
            if (value != null) {
                return System.Convert.ToSingle(value);
            }
            return null;
        }
        public static decimal? ToDecimal(object value) {
            if (value != null) {
                return System.Convert.ToDecimal(value);
            }
            return null;
        }
        public static char? ToChar(object value) {
            if (value != null) {
                return System.Convert.ToChar(value);
            }
            return null;
        }
        public static Guid? ToGuid(object value) {
            if (value != null) {
                return new System.Guid(value.ToString());
            }
            return null;
        }
        public static DateTime? ToDateTime(object value) {
            if (value != null) {
                return System.Convert.ToDateTime(value);
            }
            return null;
        }
    }
}