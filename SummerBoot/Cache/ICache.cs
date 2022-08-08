using System;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace SummerBoot.Cache
{
    public interface ICache
    {
        bool SetValueWithAbsolute<T>(string key, T value, TimeSpan absoluteExpiration);

        bool SetValueWithSliding<T>(string key, T value, TimeSpan slidingExpiration);

        CacheEntity<T> GetValue<T>(string key);

        bool Remove(string key);
    }

    public class DefaultCache : ICache
    {
        private readonly IMemoryCache memoryCache;
        private readonly ICacheDeserializer cacheDeserializer;
        private readonly ICacheSerializer cacheSerializer;

        public DefaultCache(IMemoryCache memoryCache, ICacheDeserializer cacheDeserializer, ICacheSerializer cacheSerializer)
        {
            this.memoryCache = memoryCache;
            this.cacheDeserializer = cacheDeserializer;
            this.cacheSerializer = cacheSerializer;
        }

        public CacheEntity<T> GetValue<T>(string key)
        {
            var hasValue = memoryCache.TryGetValue(key, out var obj);
            var result = new CacheEntity<T>()
            {
                HasValue = false,
                Data = default
            }; ;
            if (hasValue && obj != null)
            {
                var objPack = obj as ICacheEntry;
                if (objPack == null)
                {
                    return result;
                }
                result = cacheDeserializer.DeserializeObject<CacheEntity<T>>(objPack.Value);
            }
         

            return result;
        }

        public bool Remove(string key)
        {
            memoryCache.Remove(key);
            return true;
        }


        public bool SetValueWithAbsolute<T>(string key, T value, TimeSpan absoluteExpiration)
        {
            var entity = memoryCache.CreateEntry(key);
            CacheEntity<T> result = new CacheEntity<T>()
            {
                HasValue = true,
                Data = value
            };
            entity.Value = cacheSerializer.SerializeObject(result);
            memoryCache.Set(key, entity, new MemoryCacheEntryOptions(){AbsoluteExpirationRelativeToNow = absoluteExpiration});
            return true;
        }

        public bool SetValueWithSliding<T>(string key, T value, TimeSpan slidingExpiration)
        {
            var entity = memoryCache.CreateEntry(key);
            CacheEntity<T> result = new CacheEntity<T>()
            {
                HasValue = true,
                Data = value
            };
            entity.Value = cacheSerializer.SerializeObject(result);
            memoryCache.Set(key, entity, new MemoryCacheEntryOptions() { SlidingExpiration = slidingExpiration });
            return true;
        }
    }
}