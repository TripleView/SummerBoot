using System.Collections.Generic;
using SummerBoot.Core;

namespace SummerBoot.Cache
{
    public class SimpleCacheResolver : AbstractCacheResolver
    {
        public SimpleCacheResolver(ICacheManager cacheManager):base(cacheManager)
        {
            SbAssert.NotNull(cacheManager, "cacheManager Not be Null");
        }

        public SimpleCacheResolver()
        {

        }

        protected override ICollection<string> GetCacheNames(ICacheOperationContext<CacheOperation> context)
        {
            return new List<string>() {context.GetOperation().Name};
        }
    }
}