using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CwLib.Redis
{
    public class RedisDatabase
    {
        internal RedisDatabase(int dbnum)
        {
            db = Redis.Database(dbnum);
        }

        string _lockValue;

        IDatabase db;
        public async Task<string> GetAsync(string key)
        {
            var result = await db.StringGetAsync(key);

            return result;
        }

        public async Task<RedisValue> GetFormSlaveAsync(string key)
        {
            var result = await db.StringGetAsync(key, CommandFlags.PreferSlave);

            return result;
        }

        public async Task SetAsync(string key, RedisValue value, int timeout)
        {
            await db.StringSetAsync(key, value, new TimeSpan(0, 0, timeout));
        }

        private string GetLockKey(string lockName)
        {
            return $"{lockName}_lock";
        }
        public bool TryLock(string lockName, int lockTimeout = 20)
        {
            if (string.IsNullOrEmpty(_lockValue))
            {
                _lockValue = $"{Environment.MachineName}_{Guid.NewGuid()}_{Environment.CurrentManagedThreadId}";
                return db.LockTake(GetLockKey(lockName), _lockValue, new TimeSpan(0, 0, lockTimeout));
            }
            return false;
        }

        public bool LockRelease(string lockName)
        {
            if (string.IsNullOrEmpty(_lockValue))
            {
                return false;
            }
            return db.LockRelease(GetLockKey(lockName), _lockValue);

        }
    }
}
