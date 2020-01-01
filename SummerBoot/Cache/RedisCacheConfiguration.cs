using System;
using SummerBoot.Core;

namespace SummerBoot.Cache
{
    public class RedisCacheConfiguration
    {
        /// <summary>
        /// 过期时间,单位为秒
        /// </summary>
        public int Ttl { get; }
        /// <summary>
        /// 是否启用前缀
        /// </summary>
        public bool UsePrefix { get; }

        /// <summary>
        /// 前缀值
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// 是否允许空值
        /// </summary>
        public bool CacheNullValues { get; }

        public ISerialization Serialization { get; }

        public RedisCacheConfiguration(int ttl, bool usePrefix, string prefix, bool cacheNullValues, ISerialization serialization)
        {
            this.Ttl = ttl;
            this.UsePrefix = usePrefix;
            this.Prefix = prefix;
            this.CacheNullValues = cacheNullValues;
            this.Serialization = serialization;
        }

        public string GetKeyPrefixFor(string cacheName)
        {
            return cacheName + "::";
        }
    }
}