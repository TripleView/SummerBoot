using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SummerBoot.Core;

namespace SummerBoot.Cache
{
    public class CacheOperationContextCollection
    {
        private readonly Dictionary<Type, ICollection<CacheOperationContext>> contexts;

        private readonly ConcurrentDictionary<CacheOperationCacheKey, CacheOperationMetadata> metadataCaches;

        private readonly IServiceProvider _serviceProvider;

        public CacheOperationContextCollection(ICollection<CacheOperation> operations, MethodInfo method, object[] args, object target, Type targetType, IServiceProvider serviceProvider)
        {
            this.contexts = new Dictionary<Type, ICollection<CacheOperationContext>>(operations.Count);
            this._serviceProvider = serviceProvider;
            this.metadataCaches = new ConcurrentDictionary<CacheOperationCacheKey, CacheOperationMetadata>();

            foreach (var cacheOperation in operations)
            {
                this.contexts.TryGetValue(cacheOperation.GetType(), out var list);
                if (list == null)
                {
                    list = new List<CacheOperationContext>();
                    this.contexts.Add(cacheOperation.GetType(), list);
                }
                list.Add(GetOperationContext(cacheOperation, method, args, target, targetType));
            }

        }

        public ICollection<CacheOperationContext> Get(Type operationType)
        {
            this.contexts.TryGetValue(operationType, out var resultTmp);
            var result = resultTmp ?? new List<CacheOperationContext>();
            return result;
        }

        private CacheOperationContext GetOperationContext(
              CacheOperation operation, MethodInfo method, Object[] args, Object target, Type targetType)
        {

            CacheOperationMetadata metadata = GetCacheOperationMetadata(operation, method, targetType);
            return new CacheOperationContext(metadata, args, target);
        }

        public CacheOperationMetadata GetCacheOperationMetadata(CacheOperation operation, MethodInfo method, Type targetType)
        {
            var cacheKey = new CacheOperationCacheKey(operation, method, targetType);
            this.metadataCaches.TryGetValue(cacheKey, out var metadata);
            if (metadata == null)
            {
                IKeyGenerator keyGenerator;
                var keyGeneratorName = operation.KeyGenerator;
                if (keyGeneratorName.HasText())
                {
                    keyGenerator = this._serviceProvider.GetServiceByName<IKeyGenerator>(keyGeneratorName);
                }
                else
                {
                    keyGenerator = new SimpleKeyGenerator();
                }

                ICacheResolver cacheResolver;
                var cacheResolverName = operation.CacheResolver;
                if (cacheResolverName.HasText())
                {
                    cacheResolver = this._serviceProvider.GetServiceByName<ICacheResolver>(cacheResolverName);
                }
                else if (operation.CacheManager.HasText())
                {
                    ICacheManager cacheManager =
                        this._serviceProvider.GetServiceByName<ICacheManager>(operation.CacheManager);

                    SbAssert.NotNull(cacheManager, "cacheManager:" + operation.CacheManager + " Not Exist");

                    cacheResolver = new SimpleCacheResolver(cacheManager);
                }
                else
                {
                    cacheResolver = new SimpleCacheResolver();
                }

                metadata = new CacheOperationMetadata(operation, method, targetType, keyGenerator, cacheResolver);
            }

            return metadata;
        }
    }
}