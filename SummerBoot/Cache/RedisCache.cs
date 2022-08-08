using System;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using SummerBoot.Core;

namespace SummerBoot.Cache
{
    public class RedisCache : ICache
    {
        private ConnectionMultiplexer redis;
        private IDatabase db;
        private readonly ICacheDeserializer cacheDeserializer;
        private readonly ICacheSerializer cacheSerializer;

        public RedisCache(CacheOption cacheOption, ICacheDeserializer cacheDeserializer,
            ICacheSerializer cacheSerializer)
        {
            redis = ConnectionMultiplexer.Connect(cacheOption.RedisConnectionString);
            db = redis.GetDatabase();
            this.cacheDeserializer = cacheDeserializer;
            this.cacheSerializer = cacheSerializer;
        }

        public CacheEntity<T> GetValue<T>(string key)
        {
            var tempResult = db.StringGet(key);
           db.KeyExpire(key,);
            var result = new CacheEntity<T>()
            {
                HasValue = false,
                Data = default
            };
            if (tempResult.ToString().IsNullOrWhiteSpace())
            {
                return result;
            }

            result = cacheDeserializer.DeserializeObject<CacheEntity<T>>(tempResult);


            return result;
        }

        public bool Remove(string key)
        {
            db.KeyDelete(key);
            return true;
        }


        public bool SetValueWithAbsolute<T>(string key, T value, TimeSpan absoluteExpiration)
        {
            CacheEntity<T> result = new CacheEntity<T>()
            {
                HasValue = true,
                Data = value
            };

            var temp = cacheSerializer.SerializeObject(result).ToString();
            db.StringSet(key, temp, absoluteExpiration);
            return true;
        }

        public bool SetValueWithSliding<T>(string key, T value, TimeSpan slidingExpiration)
        {
            CacheEntity<T> result = new CacheEntity<T>()
            {
                HasValue = true,
                Data = value
            };

            var temp = cacheSerializer.SerializeObject(result).ToString();
            db.StringSet(key, temp,slidingExpiration);
            return true;
        }
    }
}