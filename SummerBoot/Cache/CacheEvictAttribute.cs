using System;

namespace SummerBoot.Cache
{
    /// <summary>
    /// 某个方法上面标记，可以在一定条件下清空缓存
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CacheEvictAttribute : Attribute
    {
        public CacheEvictAttribute(string cacheName = "", string key = "", string condition = "", bool allEntries = false, bool beforeInvocation = false, string keyGenerator = "", string cacheManager = "")
        {
            this.CacheName = cacheName;
            this.Key = key;
            this.KeyGenerator = keyGenerator;
            this.CacheManager = cacheManager;
            this.Condition = condition;
            this.AllEntries = allEntries;
            this.BeforeInvocation = beforeInvocation;
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
        /// 是否删除全部
        /// </summary>
        public bool AllEntries { private set; get; }

        /// <summary>
        /// 是否在方法执行前清空缓存
        /// </summary>
        public bool BeforeInvocation { private set; get; }

    }
}