using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using SummerBoot.Core;

namespace SummerBoot.Cache
{
    public class CacheAspectSupport
    {
        private readonly ConcurrentDictionary<CacheOperationCacheKey, CacheOperationMetadata> metadataCaches=new ConcurrentDictionary<CacheOperationCacheKey, CacheOperationMetadata>();
        private IServiceProvider _serviceProvider;
        public async Task<object> Execute(Func<object> invoker, object target, Type targetType, MethodInfo method, object[] args, IServiceProvider serviceProvider)
        {
            ICacheAttributeParser parser = new CacheAttributeParser();
            this._serviceProvider = serviceProvider;
            var operations = parser.ParseCacheAttributes(method);
            if (!operations.IsNullOrEmpty())
            {
                return await Execute(invoker, method,
                      new CacheOperationContexts(this,operations, method, args, target, targetType, serviceProvider));
            }

            return invoker();
        }

        private async Task<object> Execute(Func<object> invoker, MethodInfo method, CacheOperationContexts contexts)
        {

            //var context = contexts.InternalGet(typeof(CacheableOperation)).FirstOrDefault();
            //if (context!=null&&context.IsConditionPassing(CacheOperationExpressionEvaluator.NO_RESULT))
            //{
            //   var key = context.GenerateKey(CacheOperationExpressionEvaluator.NO_RESULT);
            //   var cache = context.Caches.FirstOrDefault();
            //   if(cache==null) return new object();
            //   return cache.InternalGet<object>(key.ToString(), null);
            //}
            //else
            //{
            //    return invoker();
            //}

            //判断方法返回值是否为异步类型
            var isAsyncReturnType = method.ReturnType.IsAsyncType();
            //返回类型
            var returnType = isAsyncReturnType ? method.ReturnType.GenericTypeArguments.First() : method.ReturnType;
            //先清除符合条件的缓存
            ProcessCacheEvicts(contexts.Get(typeof(CacheEvictOperation)), true,
            CacheOperationExpressionEvaluator.NO_RESULT);
            //从缓存中查找结果值
            var cacheHit = FindCachedItem(contexts.Get(typeof(CacheableOperation)), returnType);

            // Collect puts from any @Cacheable miss, if no cached item is found
            var cachePutRequests = new List<CachePutRequest>();
            //如果未命中，则收集这个查询
            if (cacheHit == null)
            {
                CollectPutRequests(contexts.Get(typeof(CacheableOperation)),
                CacheOperationExpressionEvaluator.NO_RESULT, cachePutRequests);
            }
            //缓存得结果值
            var cacheValue = new object();
            //如果命中，直接获得命中的结果值作为返回值
            if (cacheHit != null && !HasCachePut(contexts))
            {
                // If there are no put requests, just use the cache hit
                cacheValue = cacheHit.Get();

            }
            else
            //如果未命中，则执行具体方法，获得返回值
            {

                var cacheValueInvoker = invoker();
                if (isAsyncReturnType && cacheValueInvoker is Task task)
                {
                    await task;
                    cacheValue = (cacheValueInvoker as dynamic).Result;
                    //如果是异步结果，则获取task里的result
                    // var resultProperty = typeof(Task<>).MakeGenericType(returnType).GetProperty("Result");
                    // cacheValue = resultProperty?.GetValue(cacheValueInvoker);
                }
                else
                {
                    //同步则直接为结果
                    cacheValue = cacheValueInvoker;
                }
            }

            // 收集所有put的请求
            CollectPutRequests(contexts.Get(typeof(CachePutOperation)), cacheValue, cachePutRequests);

            //执行所有收集到的put请求，无论是CachePut还是Cacheable未命中
            foreach (var putRequest in cachePutRequests)
            {
                putRequest.Apply(cacheValue);
            }

            //执行清除请求
            ProcessCacheEvicts(contexts.Get(typeof(CacheEvictOperation)), false,
                cacheValue);

            if (isAsyncReturnType)
            {
                dynamic temp = cacheValue;
                var result = Task.FromResult(temp);
                //如果是异步方法，需要对结果值进行封装
                // var result = typeof(Task).GetMethods().First(p => p.Name == "FromResult" && p.ContainsGenericParameters)
                //     .MakeGenericMethod(returnType).Invoke(null, new object[] { cacheValue });
                return result;
            }

            return cacheValue;
        }

        private bool HasCachePut(CacheOperationContexts contexts)
        {
            // Evaluate the conditions *without* the result object because we don't have it yet...
            ICollection<CacheOperationContext> cachePutContexts = contexts.Get(typeof(CachePutOperation));
            ICollection<CacheOperationContext> excluded = new List<CacheOperationContext>();
            foreach (var context in cachePutContexts)
            {
                try
                {
                    if (!context.IsConditionPassing(CacheOperationExpressionEvaluator.NO_RESULT))
                    {
                        excluded.Add(context);
                    }
                }
                catch (Exception e)
                {

                }
            }

            // Check if all puts have been excluded by condition
            return cachePutContexts.Count != excluded.Count;
        }

        private void CollectPutRequests(ICollection<CacheOperationContext> contexts,
           object result, ICollection<CachePutRequest> putRequests)
        {

            foreach (var context in contexts)
            {
                if (context.IsConditionPassing(result))
                {
                    var key = context.GenerateKey(result);
                    putRequests.Add(new CachePutRequest(context, key));
                }
            }
        }

        private void ProcessCacheEvicts(ICollection<CacheOperationContext> contexts, bool beforeInvocation, object result)
        {
            foreach (var context in contexts)
            {
                CacheEvictOperation operation = (CacheEvictOperation)context.Metadata.Operation;
                if (beforeInvocation == operation.BeforeInvocation && context.IsConditionPassing(result))
                {
                    performCacheEvict(context, operation, result);
                }
            }
        }

        private void performCacheEvict(
            CacheOperationContext context, CacheEvictOperation operation, object result)
        {

            var key = "";
            foreach (var cache in context.Caches)
            {
                if (operation.CacheWide)
                {
                    cache.Clear();
                }
                else
                {
                    if (key.IsNullOrWhiteSpace())
                    {
                        key = context.GenerateKey(result);
                    }
                    cache.Evict(key);
                }
            }

        }

        private IValueWrapper FindCachedItem(ICollection<CacheOperationContext> contexts, Type returnType)
        {
            var noResult = CacheOperationExpressionEvaluator.NO_RESULT;
            foreach (var context in contexts)
            {
                if (context.IsConditionPassing(noResult))
                {
                    string key = context.GenerateKey(noResult);
                    var result = FindInCaches(context, key, returnType);
                    if (result != null) return result;
                }
            }

            return null;
        }

        private IValueWrapper FindInCaches(CacheOperationContext context, string key, Type returnType)
        {
            foreach (var cache in context.Caches)
            {
                var result = cache.Get(key, returnType);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
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

        private class CacheOperationContexts
        {
            private readonly Dictionary<Type, ICollection<CacheOperationContext>> contexts;

            public CacheOperationContexts(CacheAspectSupport parent,ICollection<CacheOperation> operations, MethodInfo method, object[] args, object target, Type targetType, IServiceProvider serviceProvider)
            {
                this.contexts = new Dictionary<Type, ICollection<CacheOperationContext>>(operations.Count);

                foreach (var cacheOperation in operations)
                {
                    this.contexts.TryGetValue(cacheOperation.GetType(), out var list);
                    if (list == null)
                    {
                        list = new List<CacheOperationContext>();
                        this.contexts.Add(cacheOperation.GetType(), list);
                    }
                    list.Add(parent.GetOperationContext(cacheOperation, method, args, target, targetType));
                }

            }

            public ICollection<CacheOperationContext> Get(Type operationType)
            {
                this.contexts.TryGetValue(operationType, out var resultTmp);
                var result = resultTmp ?? new List<CacheOperationContext>();
                return result;
            }

          
        }

        public class CacheOperationContext : ICacheOperationContext<CacheOperation>
        {
            public CacheOperationMetadata Metadata { get; }

            public object[] Args { get; }

            public object Target { get; }

            public ICollection<ICache> Caches { get; }

            public ICollection<string> CacheNames { get; }

            private bool? conditionPassing;

            public CacheOperationContext(CacheOperationMetadata metadata, object[] args, object target)
            {
                this.Metadata = metadata;
                this.Args = args;
                this.Target = target;
                this.Caches = GetCaches(this, Metadata.CacheResolver);
                this.CacheNames = this.Caches?.Select(it => it.Name).ToList();
            }

            protected ICollection<ICache> GetCaches(ICacheOperationContext<CacheOperation> context, ICacheResolver cacheResolver)
            {
                var result = cacheResolver.CacheResolve(context);
                if (result.IsNullOrEmpty())
                {
                    throw new Exception("No cache could be resolved for '" +
                                        context.GetOperation() + "' using resolver '" + cacheResolver +
                                        "'. At least one cache should be provided per cache operation.");
                }
                return result;
            }

            public bool IsConditionPassing(object result)
            {
                if (this.conditionPassing == null)
                {
                    var condition = this.Metadata.Operation.Condition;
                    if (condition.HasText())
                    {
                        this.conditionPassing = new CacheOperationExpressionEvaluator().Condition(condition, this, result);
                    }
                    else
                    {
                        return true;
                    }
                }

                return this.conditionPassing.Value;
            }

            public bool CanPutToCache(object result)
            {
                string unless = "";
                if (this.Metadata.Operation is CacheableOperation cacheableOperation)
                {
                    unless = cacheableOperation.Unless;
                }

                else if (this.Metadata.Operation is CachePutOperation cachePutOperation)
                {
                    unless = cachePutOperation.Unless;
                }
                if (unless.HasText())
                {
                    return new CacheOperationExpressionEvaluator().Unless(unless, this, result);
                }
                return true;
            }

            public object[] GetArgs()
            {
                return this.Args;
            }

            public MethodInfo GetMethod()
            {
                return this.Metadata.Method;
            }

            public CacheOperation GetOperation()
            {
                return this.Metadata.Operation;
            }

            public object GetTarget()
            {
                return this.Target;
            }

            public string GenerateKey(object result)
            {
                if (this.Metadata.Operation.Key.HasText())
                {
                    return this.Metadata.Operation.Key;
                }
                return this.Metadata.KeyGenerator.Generate(this.Target, this.Metadata.Method, this.Args);
            }

        }

        private class CachePutRequest
        {
            private readonly CacheOperationContext context;
            private readonly object key;

            public CachePutRequest(CacheOperationContext context, object key)
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
                        cache.Put(this.key.ToString(), result);
                    }
                }
            }
        }
    }


}