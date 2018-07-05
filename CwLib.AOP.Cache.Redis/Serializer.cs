using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;

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
}
