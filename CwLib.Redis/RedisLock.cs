using System;
using System.Diagnostics;
using System.Threading;
using StackExchange.Redis;

namespace CwLib.Redis
{
    public class RedisLock : IDisposable
    {
        string _lockKey;
        string _lockValue;

        IDatabase database;

        internal RedisLock(string lockName, int lockTimeout, int db)
        {
            database = Redis.Database(db);
            _lockKey = $"{lockName}_lock";
            _lockValue = $"{Environment.MachineName}_{Guid.NewGuid()}_{Environment.CurrentManagedThreadId}";

            while (!database.LockTake(_lockKey, _lockValue, new TimeSpan(0, 0, lockTimeout)))
            {
                SpinWait.SpinUntil(() => false, 200);
            }
        }

        public void Dispose()
        {
            if (!database.LockRelease(_lockKey, _lockValue))
            {
                Debug.WriteLine($"Redis lock timeout.(Key: {_lockKey}, Value: {_lockValue})");
            }
        }
    }
}
