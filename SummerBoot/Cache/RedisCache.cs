using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using SummerBoot.Core;

namespace SummerBoot.Cache
{
    public class RedisCache : ICache
    {

        private readonly ICacheDeserializer cacheDeserializer;
        private readonly ICacheSerializer cacheSerializer;
        private readonly IDistributedCache distributedCache;

        public RedisCache(ICacheDeserializer cacheDeserializer, ICacheSerializer cacheSerializer, IDistributedCache distributedCache)
        {
            this.cacheDeserializer = cacheDeserializer;
            this.cacheSerializer = cacheSerializer;
            this.distributedCache = distributedCache;
        }

        public CacheEntity<T> GetValue<T>(string key)
        {
            var bytes = distributedCache.Get(key);
            var result = ChangeBytesToResult<T>(bytes);
            return result;
        }

        private CacheEntity<T> ChangeBytesToResult<T>(byte[] bytes)
        {
            var result = new CacheEntity<T>()
            {
                HasValue = false,
                Data = default
            };
            if (bytes == null || bytes.Length == 0)
            {
                return result;
            }

            result = cacheDeserializer.DeserializeObject<CacheEntity<T>>(bytes);

            return result;
        }

        public async Task<CacheEntity<T>> GetValueAsync<T>(string key)
        {
            var bytes = await distributedCache.GetAsync(key);
            var result = ChangeBytesToResult<T>(bytes);
            return result;
        }

        public bool Remove(string key)
        {
            distributedCache.Remove(key);
            return true;
        }

        public async Task<bool> RemoveAsync(string key)
        {
            await distributedCache.RemoveAsync(key);
            return true;
        }

        public bool SetValueWithAbsolute<T>(string key, T value, TimeSpan absoluteExpiration)
        {
            var temp = ChangeValueToBytes(value);
            distributedCache.Set(key, temp, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = absoluteExpiration });
            return true;
        }

        private byte[] ChangeValueToBytes<T>(T value)
        {
            CacheEntity<T> result = new CacheEntity<T>()
            {
                HasValue = true,
                Data = value
            };

            var temp = cacheSerializer.SerializeObject(result);
            return temp;
        }

        public async Task<bool> SetValueWithAbsoluteAsync<T>(string key, T value, TimeSpan absoluteExpiration)
        {
            var temp = ChangeValueToBytes(value);
            await distributedCache.SetAsync(key, temp, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = absoluteExpiration });
            return true;
        }

        public bool SetValueWithSliding<T>(string key, T value, TimeSpan slidingExpiration)
        {
            var temp = ChangeValueToBytes(value);

            distributedCache.Set(key, temp, new DistributedCacheEntryOptions() { SlidingExpiration = slidingExpiration });
            return true;
        }

        public async Task<bool> SetValueWithSlidingAsync<T>(string key, T value, TimeSpan slidingExpiration)
        {
            var temp = ChangeValueToBytes(value);

            await distributedCache.SetAsync(key, temp, new DistributedCacheEntryOptions() { SlidingExpiration = slidingExpiration });
            return true;
        }
    }
}