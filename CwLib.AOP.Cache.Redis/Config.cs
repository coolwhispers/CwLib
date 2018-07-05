using System;

namespace CwLib.AOP.Cache.Redis
{
    public class Config
    {
        public static int DefualtRedisDb = -1;
        public static int LockWaitTime { get; set; } = 200;

        public static ISerializer Serializer { get; set; } = new JsonFormatter();
    }
}
