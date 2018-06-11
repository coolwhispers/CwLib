using System;
using StackExchange.Redis;

namespace CwLib.Redis
{
    public class Redis
    {
        private static readonly Lazy<ConnectionMultiplexer> _connection;

        public static ConnectionMultiplexer GetConnection() => _connection.Value;

        public IDatabase Database(int db = -1)
        {
            return GetConnection().GetDatabase(db);
        }

        static Redis()
        {
            var connectionString =
               System.Configuration.ConfigurationManager.AppSettings["RedisConnection"];
            var options = ConfigurationOptions.Parse(connectionString);
            _connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(options));
        }


    }
}
