using Newtonsoft.Json;
using StackExchange.Redis;
using SummerBoot.Core;
using System;

namespace SummerBoot.Cache
{
    public class RedisCache : ICache
    {
        public string Name { get; }

        private readonly RedisCacheConfiguration redisCacheConfiguration;

        private readonly IRedisCacheWriter redisCacheWriter;

        private static readonly byte[] BINARY_NULL_VALUE = new byte[0];

        public RedisCache(string name, RedisCacheConfiguration configuration, IRedisCacheWriter redisCacheWriter)
        {
            SbAssert.NotNull(configuration, "redis 配置RedisCacheConfiguration不能为空");
            SbAssert.NotNull(redisCacheWriter, "RedisCacheWriter不能为空");

            this.Name = name;
            this.redisCacheWriter = redisCacheWriter;
            this.redisCacheConfiguration = configuration;
        }

        public object GetNativeCache
        {
            get => redisCacheWriter;
        }

        public void Clear()
        {
            byte[] pattern = CreateCacheKey("*").GetBytes();
            redisCacheWriter.Clean(Name, pattern);
        }

        public void Evict(string key)
        {
            redisCacheWriter.Clean(Name, key.GetBytes());
        }

        public T Get<T>(string key, Func<T> callBack = null)
        {
            string dbResult = redisCacheWriter.Get(Name, key.GetBytes()).GetString(); ;

            //返回值
            T result = default;

            var returnType = typeof(T);
            //value不为空，转为T后直接返回值
            if (!dbResult.IsNullOrWhiteSpace())
            {
                result = (T)redisCacheConfiguration.Serialization.DeserializeObject(dbResult, returnType);
            }
            //否则执行回调函数，获得返回值后，put进缓存，最后return返回值
            else if (callBack != null)
            {
                result = callBack();
                this.Put(key, result);
            }

            return result;
        }

        public void Put(string key, object value)
        {
            //json序列化
            var valueStr = redisCacheConfiguration.Serialization.SerializeObject(value);
            redisCacheWriter.Put(Name, key.GetBytes(), valueStr.GetBytes(), TimeSpan.FromSeconds(redisCacheConfiguration.Ttl));
        }

        /// <summary>
        /// 获得key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected string CreateCacheKey(string key)
        {
            return redisCacheConfiguration.UsePrefix ? PrefixCacheKey(key) : key;
        }

        /// <summary>
        /// 前缀+真正key，获得完整的key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string PrefixCacheKey(string key)
        {
            return redisCacheConfiguration.GetKeyPrefixFor(Name) + key;
        }

        public IValueWrapper Get(string key, Type returnType, Func<object> callBack = null)
        {
            string dbResult = redisCacheWriter.Get(Name, key.GetBytes()).GetString(); ;

            //返回值
            IValueWrapper result = null;

            //value不为空，转为T后直接返回值
            if (!dbResult.IsNullOrWhiteSpace())
            {

                var obj = redisCacheConfiguration.Serialization.DeserializeObject(dbResult, returnType);
                result = new ValueWrapper(obj);
            }
            //否则执行回调函数，获得返回值后，put进缓存，最后return返回值
            else if (callBack != null)
            {
                var objTmp = callBack();
                this.Put(key, objTmp);
                result = new ValueWrapper(objTmp);
            }

            return result;
        }
    }
}