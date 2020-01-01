using System;
using System.Collections.Generic;
using SummerBoot.Core;

namespace SummerBoot.Cache
{
    public abstract class AbstractCacheResolver : ICacheResolver
    {
        public ICacheManager CacheManager { get; }

        protected AbstractCacheResolver(ICacheManager cacheManager)
        {
            this.CacheManager = cacheManager;
            SbAssert.NotNull(CacheManager, "cacheManager Not be Null");
        }

        protected AbstractCacheResolver() { }

        public ICollection<ICache> CacheResolve(ICacheOperationContext<CacheOperation> context)
        {
            ICollection<string> cacheNames = GetCacheNames(context);

            if (cacheNames == null) return new List<ICache>();

            var result = new List<ICache>();

            foreach (var cacheName in cacheNames)
            {
                var cache = CacheManager.GetCache(cacheName);
                if (cache == null)
                {
                    throw new Exception("Cannot find cache name '" +
                                        cacheName + "' for " + context.GetOperation());
                }
                result.Add(cache);
            }

            return result;
        }

        protected abstract ICollection<string> GetCacheNames(ICacheOperationContext<CacheOperation> context);

    }
}