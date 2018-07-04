using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using CwLib.AOP;

namespace CwLib.AOP.Cache.Redis
{
    public interface ISerializer
    {
        byte[] Serialize(object data);
        object Deserialize(byte[] data, Type type);
    }

    class JsonFormatter : ISerializer
    {
        public bool UseJsonDotNet { get; }

        private MethodInfo SerializerMethod;
        private MethodInfo DeserializeMethod;

        private bool JsonDotNetInit()
        {
            try
            {
                var jsonDotNet = Assembly.Load(new AssemblyName("Newtonsoft.Json"));
                var jsonConvertType = jsonDotNet.ExportedTypes.Where(x => x.FullName == "Newtonsoft.Json.JsonConvert").FirstOrDefault();
                var jsonConvertTypeInfo = jsonConvertType.GetTypeInfo();
                DeserializeMethod = jsonConvertTypeInfo.GetDeclaredMethods("DeserializeObject").FirstOrDefault(x => x.IsGenericMethodDefinition && x.GetParameters().Length == 1);
                SerializerMethod = jsonConvertTypeInfo.GetDeclaredMethods("SerializeObject").FirstOrDefault(x => x.GetParameters().Length == 1);

                return DeserializeMethod != null && SerializerMethod != null;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);

                return false;
            }
        }

        private string JsonDotNetSerializer(object obj)
        {
            return (string) SerializerMethod.Invoke(null, new [] { obj });
        }

        private object JsonDotNetDeserialize(string json, Type type)
        {
            var method = DeserializeMethod.MakeGenericMethod(type);
            return method.Invoke(null, new [] { json });
        }

        private string JsonSerializer(object obj)
        {
            var ms = new MemoryStream();

            var serializer = new DataContractJsonSerializer(obj.GetType());

            serializer.WriteObject(ms, obj);

            var reader = new StreamReader(ms);

            return reader.ReadToEnd();
        }

        private object JsonDeserialize(string json, Type type)
        {
            var jsonBytes = Encoding.UTF8.GetBytes(json);

            var ms = new MemoryStream(jsonBytes);

            var serializer = new DataContractJsonSerializer(type);

            return serializer.ReadObject(ms);
        }

        public JsonFormatter()
        {
            UseJsonDotNet = JsonDotNetInit();
        }

        public bool DataCheck(object obj)
        {
            return obj != null;
        }

        public object Deserialize(byte[] data, Type type)
        {
            var json = System.Text.Encoding.UTF8.GetString(data);

            return UseJsonDotNet ? JsonDotNetDeserialize(json, type) : JsonDeserialize(json, type);
        }

        public byte[] Serialize(object data)
        {
            var json = UseJsonDotNet ? JsonDotNetSerializer(data) : JsonSerializer(data);

            return Encoding.UTF8.GetBytes(json);
        }
    }

    public class CwCacheNamespaceAttribute : Attribute
    {
        public string Namespace { get; set; }
        public CwCacheNamespaceAttribute(string nameSpace)
        {
            Namespace = nameSpace;
        }
    }

    public class CwRedisCacheAttribute : CwAopAttribute
    {
        protected int WaitTime { get; set; } = 20;
        public static ISerializer Serializer { get; set; } = new JsonFormatter();
        protected static int redisDb = -1;
        public static int CacheRedisDb { get; set; } = -1;
        public static int LockWaitTime { get; set; } = 20;

        public string Namespace { get; set; }

        public int RedisDb { get; set; } = -1;

        protected CwLib.Redis.RedisDatabase db;

        protected string CreateKey()
        {
            var source = Serializer.Serialize(AopAction.Args);

            var provider = new MD5CryptoServiceProvider();
            var crypto = provider.ComputeHash(source);
            var result = BitConverter.ToString(crypto);

            return $"{CreateNamespaceString()}{AopAction.TargetMethod.Name}_{result}";
        }

        protected string CreateNamespaceString()
        {
            var objectName = typeof(object).Name;
            var customAttribute = typeof(CwCacheNamespaceAttribute);
            var method = AopAction.TargetMethod.DeclaringType;
            var classNamespace = method.FullName;

            while (string.IsNullOrWhiteSpace(Namespace) && method.Name != objectName)
            {
                var attribute = (CwCacheNamespaceAttribute) method.GetCustomAttribute(customAttribute);

                if (attribute != null)
                {
                    Namespace = attribute.Namespace;
                    break;
                }

                method = method.BaseType;
            }

            return $"{Namespace}_";
        }
    }

    public class CwCacheMethodAttribute : CwRedisCacheAttribute
    {
        public int Timeout { get; set; } = 600;

        object _result = null;
        string key;

        bool _hasCache = false;

        public override void OnBegin()
        {
            if (string.IsNullOrWhiteSpace(Namespace)) { }

            key = CreateKey();

            db = CwLib.Redis.Redis.GetDatabase(RedisDb == -1 ? redisDb : RedisDb);

            _result = Serializer.Deserialize(db.GetFormSlaveAsync(key).Result, AopAction.TargetMethod.ReturnType);

            if (_result != null)
            {
                _hasCache = true;
                AopAction.ExecuteMethod = EcecuteType.None;
                return;
            }

            var waitTime = Stopwatch.StartNew();

            while (!db.TryLock(key))
            {
                if (waitTime.Elapsed.TotalSeconds > LockWaitTime)
                {
                    break;
                }
                SpinWait.SpinUntil(() => false, WaitTime);
            }

            waitTime.Stop();
        }

        public override void OnResult(ref object result)
        {
            if (_hasCache)
            {
                result = _result;
            }
            else
            {
                db.SetAsync(key, Serializer.Serialize(_result), Timeout).Wait();
            }

            db.LockRelease(key);
        }

        public override void OnExecption(Exception e) { }

        public override void Dispose() { }
    }
}
