namespace SummerBoot.Cache
{
    public abstract class CacheOperation : ICacheOperation
    {

        protected CacheOperation(string name, string cacheName, string key, string condition = "", string keyGenerator = "", string cacheManager = "", string cacheResolver = "")
        {
            this.CacheName = cacheName;
            this.Key = key;
            this.KeyGenerator = keyGenerator;
            this.CacheManager = cacheManager;
            this.Condition = condition;
            this.Name = name;
            this.CacheResolver = cacheResolver;
        }

        /// <summary>
        /// 操作的名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 缓存的名称
        /// </summary>
        public string CacheName { get; }

        /// <summary>
        /// 键
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// 键生成器
        /// </summary>
        public string KeyGenerator { get; }

        /// <summary>
        /// 缓存管理器
        /// </summary>
        public string CacheManager { get; }

        /// <summary>
        /// 缓存解析器
        /// </summary>
        public string CacheResolver { get; }

        /// <summary>
        /// 符合Condition条件才缓存
        /// </summary>
        public string Condition { get; }

        public string GetCacheName()
        {
            return this.CacheName;
        }

        public override string ToString()
        {
            var result = string.Empty;
            result += "[" + this.Name;
            result += "] cache=" + this.CacheName;
            result += " | key='" + this.Key;
            result += " | keyGenerator='" + this.KeyGenerator;
            result += " | cacheManager='" + this.CacheManager;
            result += " | condition='" + this.Condition;
            result += " | cacheResolver='" + this.CacheResolver;

            return result;
        }
    }
}