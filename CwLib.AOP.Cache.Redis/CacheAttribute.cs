using System;
using System.Diagnostics;
using System.Threading;
using CwLib.AOP;

namespace CwLib.AOP.Cache.Redis
{
    public interface ISerializer
    {
        byte[] Serialize(object data);
        object Deserialize(byte[] data);
    }

    class Serializer : ISerializer
    {
        public byte[] Serialize(object data)
        {
            //TODO:
            return null;
        }

        public object Deserialize(byte[] data)
        {
            //TODO:
            return null;
        }
    }

    public class CwRedisCacheAttribute : CwAopAttribute
    {
        protected int WaitTime { get; set; } = 20;
        public static ISerializer Serializer { get; set; } = new Serializer();
        protected static int redisDb = -1;
        public static int CacheRedisDb { get; set; } = -1;
        public static int LockWaitTime { get; set; } = 20;

        public string Namespace { get; set; }

        public int RedisDb { get; set; } = -1;

        protected CwLib.Redis.RedisDatabase db;

        protected string CreateKey()
        {
            //TODO:
            return $"{CreateNamespaceKey()}{AopAction.TargetMethod.Name}_{1234567890}";
        }

        protected string CreateNamespaceKey()
        {
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

            _result = Serializer.Deserialize(db.GetFormSlaveAsync(key).Result);

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
