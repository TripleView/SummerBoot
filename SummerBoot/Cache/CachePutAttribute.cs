using System;

namespace SummerBoot.Cache
{
    /// <summary>
    /// 常用于标记某个方法，根据方法的请求参数对其结果进行缓存，和 Cacheable 不同的是，它每次都会触发真实方法的调用 。简单来说就是用户更新缓存数据
    /// </summary>
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = true)]
    public class CachePutAttribute : Attribute
    {
        public CachePutAttribute(string cacheName = "", string key = "", string condition = "", string unless = "", string keyGenerator = "", string cacheManager = "")
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
        /// 符合Condition条件才缓存
        /// </summary>
        public string Condition { private set; get; }

        /// <summary>
        /// 符合Unless条件不缓存
        /// </summary>
        public string Unless { private set; get; }
    }
}