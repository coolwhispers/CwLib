using System;
using StackExchange.Redis;

namespace CwLib.Redis
{
    public class Redis
    {
        private static readonly Lazy<ConnectionMultiplexer> _connection;

        public static ConnectionMultiplexer GetConnection() => _connection.Value;

        public static IDatabase Database(int db = -1)
        {
            return GetConnection().GetDatabase(db);
        }

        public static RedisDatabase GetDatabase(int db = -1)
        {
            return new RedisDatabase(db);
        }

        static Redis()
        {
            _connection = new Lazy<ConnectionMultiplexer>(() =>
            {
                var connectionString =
                    System.Configuration.ConfigurationManager.AppSettings["RedisConnection"];
                var options = ConfigurationOptions.Parse(connectionString);
                return ConnectionMultiplexer.Connect(options);
            });
        }

        public static IDisposable Lock(int lockTimeout = 20, int db = -1)
        {
            var id = Guid.NewGuid().ToString().ToUpper();

            return Lock(id, lockTimeout, db);
        }
        public static IDisposable Lock(string lockName, int lockTimeout = 20, int db = -1)
        {
            return new RedisLock(lockName, lockTimeout, db);
        }

    }
}
