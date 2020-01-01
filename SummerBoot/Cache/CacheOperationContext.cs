using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SummerBoot.Core;

namespace SummerBoot.Cache
{
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
            if (this.conditionPassing==null)
            {
                var condition = this.Metadata.Operation.Condition;
                if (condition.HasText())
                {
                    this.conditionPassing = new CacheOperationExpressionEvaluator().Condition(condition,this,result);
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
}