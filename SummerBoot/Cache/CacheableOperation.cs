namespace SummerBoot.Cache
{
    public class CacheableOperation : CacheOperation
    {
        public CacheableOperation(string name, string cacheName, string key, string condition = "", string keyGenerator = "", string cacheManager = "", string unless = "") : base(name, cacheName, key, condition, keyGenerator, cacheManager)
        {
            this.Unless = unless;
        }

        /// <summary>
        /// 符合Unless条件不缓存
        /// </summary>
        public string Unless { private set; get; }

        public override string ToString()
        {
            return base.ToString() + " | unless='" + this.Unless + "'";
        }
    }
}