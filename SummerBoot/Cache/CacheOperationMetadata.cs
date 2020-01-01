using System;
using System.Reflection;

namespace SummerBoot.Cache
{
    public sealed class CacheOperationMetadata
    {
        public CacheOperation Operation { get; }

        public MethodInfo Method { get; }

        public Type TargeType { get; }

        public IKeyGenerator KeyGenerator { get; }

        public ICacheResolver CacheResolver { get; }

        public CacheOperationMetadata(CacheOperation operation, MethodInfo method, Type targeType, IKeyGenerator keyGenerator, ICacheResolver cacheResolver)
        {
            this.Operation = operation;
            this.Method = method;
            this.TargeType = targeType;
            this.KeyGenerator = keyGenerator;
            this.CacheResolver = cacheResolver;
        }

    }
}