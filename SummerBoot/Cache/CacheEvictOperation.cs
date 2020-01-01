namespace SummerBoot.Cache
{
    public class CacheEvictOperation : CacheOperation
    {
        public CacheEvictOperation(string name, string cacheName, string key, string condition = "", string keyGenerator = "", string cacheManager = "", bool cacheWide=false,bool beforeInvocation=false) : base(name, cacheName, key, condition, keyGenerator, cacheManager)
        {
            this.CacheWide = cacheWide;
            this.BeforeInvocation = beforeInvocation;
        }

        public bool CacheWide { private set; get; }

        /// <summary>
        /// 方法执行前删除
        /// </summary>
        public bool BeforeInvocation { private set; get; }

        public override string ToString()
        {
            return base.ToString() + "，" + this.CacheWide+","+this.BeforeInvocation;
        }
    }
}