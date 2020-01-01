namespace SummerBoot.Cache
{
    public class CachePutRequest
    {
        private readonly CacheAspectSupport.CacheOperationContext context;
        private readonly object key;

        public CachePutRequest(CacheAspectSupport.CacheOperationContext context,object key)
        {
            this.context = context;
            this.key = key;
        }

        public void Apply(object result)
        {
            if (this.context.CanPutToCache(result))
            {
                foreach (var cache in this.context.Caches)
                {
                    cache.Put(this.key.ToString(),result);
                }
            }
        }
    }
}