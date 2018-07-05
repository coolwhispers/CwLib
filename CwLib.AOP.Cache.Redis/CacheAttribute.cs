using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;

namespace CwLib.AOP.Cache.Redis
{
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
        public string Namespace { get; set; }
        public int RedisDb { get; set; } = -1;

        protected CwLib.Redis.RedisDatabase db;

        protected string CreateKey()
        {
            var source = Config.Serializer.Serialize(AopAction.Args);

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
        protected int WaitTime { get; set; } = 20;

        object _result = null;
        string _key;
        bool _hasCache = false;

        public override void OnBegin()
        {
            if (string.IsNullOrWhiteSpace(Namespace)) { }

            _key = CreateKey();

            db = CwLib.Redis.Redis.GetDatabase(RedisDb == -1 ? Config.DefualtRedisDb : RedisDb);

            _result = Config.Serializer.Deserialize(db.GetFormSlaveAsync(_key).Result, AopAction.TargetMethod.ReturnType);

            if (_result != null)
            {
                _hasCache = true;
                AopAction.ExecuteMethod = EcecuteType.None;
                return;
            }

            var waitTime = Stopwatch.StartNew();

            while (!db.TryLock(_key))
            {
                if (waitTime.Elapsed.TotalSeconds > WaitTime)
                {
                    break;
                }
                SpinWait.SpinUntil(() => false, Config.LockWaitTime);
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
                db.SetAsync(_key, Config.Serializer.Serialize(_result), Timeout).Wait();
            }

            db.LockRelease(_key);
        }

        public override void OnExecption(Exception e) { }

        public override void Dispose() { }
    }
}
