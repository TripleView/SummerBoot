using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using SummerBoot.Core;

namespace SummerBoot.Cache
{
    public class RedisCacheManager : AbstractTransactionSupportingCacheManager
    {
        private bool KeyNoExistThenCreate = true;

        private IRedisCacheWriter RedisCacheWriter { set; get; }


        private ISerialization Serialization { set; get; }

        [Value("redisConfig")]
        public RedisCacheConfiguration RedisCacheConfiguration { set; get; }


        public RedisCacheManager(IUnitOfWork uow, IRedisCacheWriter redisCacheWriter, ISerialization serialization) : base(uow)
        {
            this.RedisCacheWriter = redisCacheWriter;
            this.Serialization = serialization;
        }

        public override string GetName()
        {
            return "redis";
        }

        protected override IList<ICache> LoadCaches()
        {
            var result = new List<ICache>();

            return result;
        }

        protected override ICache GetMissingCache(string name)
        {

            return CreateNewCache(name);
        }

        private ICache CreateNewCache(string name)
        {
            if (RedisCacheConfiguration == null)
            {
                RedisCacheConfiguration = new RedisCacheConfiguration(180, true, "tx", true, Serialization);
            }

            return new RedisCache(name, RedisCacheConfiguration, RedisCacheWriter);
        }

        public override void AfterPropertiesSet()
        {
            base.InitializeCaches();
            this.SetTransactionAware(true);
        }
    }
}