using System;

namespace SummerBoot.Cache
{
    /// <summary>
    /// 标记在某个方法上，缓存该方法的结果
    /// </summary>
    [AttributeUsage( AttributeTargets.Method)]
    public class CacheableAttribute : Attribute
    {
        public CacheableAttribute(string cacheName, string key, string condition = "", string unless = "", string keyGenerator = "", string cacheManager = "")
        {
            this.CacheName = cacheName;
            this.Key = key;
            this.KeyGenerator = keyGenerator;
            this.CacheManager = cacheManager;
            this.Condition = condition;
            this.Unless = unless;
        }
        /// <summary>
        /// 缓存的名称
        /// </summary>
        public string CacheName { private set; get; }

        /// <summary>
        /// 键
        /// </summary>
        public string Key { private set; get; }

        /// <summary>
        /// 键生成器
        /// </summary>
        public string KeyGenerator { private set; get; }

        /// <summary>
        /// 缓存管理器
        /// </summary>
        public string CacheManager { private set; get; }

        /// <summary>
        /// 条件符合则缓存
        /// </summary>
        public string Condition { private set; get; }

        /// <summary>
        /// 条件符合则不缓存
        /// </summary>
        public string Unless { private set; get; }
    }
}