using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Cache;
using SummerBoot.Core;
using SummerBoot.Test.Feign;
using Xunit;

namespace SummerBoot.Test.Cache
{
    public class CacheTest
    {
        [Fact]
        public async Task TestNoneValue()
        {
            var services = new ServiceCollection();

            services.AddSummerBoot();
            services.AddSummerBootCache();

            var serviceProvider = services.BuildServiceProvider();
            var cache = serviceProvider.GetRequiredService<ICache>();

            var value = cache.GetValue<string>("test");
            Assert.False(value.HasValue);
            Assert.Null(value.Data);
        }

        [Fact]
        public async Task TestSetValueWithAbsolute()
        {
            var services = new ServiceCollection();

            services.AddSummerBoot();
            services.AddSummerBootCache();

            var serviceProvider = services.BuildServiceProvider();
            var cache = serviceProvider.GetRequiredService<ICache>();
            cache.SetValueWithAbsolute("test", "test", TimeSpan.FromSeconds(3));
            await Task.Delay(2000);
            var value = cache.GetValue<string>("test");
            Assert.True(value.HasValue);
            Assert.Equal("test", value.Data);
            await Task.Delay(1000);
            value = cache.GetValue<string>("test");
            Assert.False(value.HasValue);
            Assert.Null(value.Data);
        }

        [Fact]
        public async Task TestSetValueWithSliding()
        {
            var services = new ServiceCollection();

            services.AddSummerBoot();
            services.AddSummerBootCache();

            var serviceProvider = services.BuildServiceProvider();
            var cache = serviceProvider.GetRequiredService<ICache>();
            cache.SetValueWithSliding("test", "test", TimeSpan.FromSeconds(3));
            await Task.Delay(2000);
            var value = cache.GetValue<string>("test");
            Assert.True(value.HasValue);
            Assert.Equal("test", value.Data);
            await Task.Delay(3000);
            value = cache.GetValue<string>("test");
            Assert.False(value.HasValue);
            Assert.Null(value.Data);
        }

        [Fact]
        public async Task TestRemove()
        {
            var services = new ServiceCollection();

            services.AddSummerBoot();
            services.AddSummerBootCache();

            var serviceProvider = services.BuildServiceProvider();
            var cache = serviceProvider.GetRequiredService<ICache>();
            cache.SetValueWithAbsolute("test", "test", TimeSpan.FromSeconds(3));
            await Task.Delay(2000);
            var value = cache.GetValue<string>("test");
            Assert.True(value.HasValue);
            Assert.Equal("test", value.Data);
            cache.Remove("test");
            value = cache.GetValue<string>("test");
            Assert.False(value.HasValue);
            Assert.Null(value.Data);
        }

        private ServiceCollection InitRedisServiceCollection()
        {
            var services = new ServiceCollection();

            services.AddSummerBoot();
            services.AddSummerBootCache(it=>
            {
                it.UseRedis=true;
                it.RedisConnectionString = "192.168.50.59:6379";
            });
            return services;
        }
        
         [Fact]
        public async Task TestRedisNoneValue()
        {

            var services = InitRedisServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var cache = serviceProvider.GetRequiredService<ICache>();

            var value = cache.GetValue<string>("test");
            Assert.False(value.HasValue);
            Assert.Null(value.Data);
        }

        [Fact]
        public async Task TestRedisSetValueWithAbsolute()
        {
            var services = InitRedisServiceCollection();

            var serviceProvider = services.BuildServiceProvider();
            var cache = serviceProvider.GetRequiredService<ICache>();
            cache.SetValueWithAbsolute("test", "test", TimeSpan.FromSeconds(3));
            await Task.Delay(2000);
            var value = cache.GetValue<string>("test");
            Assert.True(value.HasValue);
            Assert.Equal("test", value.Data);
            await Task.Delay(1000);
            value = cache.GetValue<string>("test");
            Assert.False(value.HasValue);
            Assert.Null(value.Data);
        }

        [Fact]
        public async Task TestRedisSetValueWithSliding()
        {
            var services = InitRedisServiceCollection();

            var serviceProvider = services.BuildServiceProvider();
            var cache = serviceProvider.GetRequiredService<ICache>();
            cache.SetValueWithSliding("test", "test", TimeSpan.FromSeconds(3));
            await Task.Delay(2000);
            var value = cache.GetValue<string>("test");
            Assert.True(value.HasValue);
            Assert.Equal("test", value.Data);
            await Task.Delay(3000);
            value = cache.GetValue<string>("test");
            Assert.False(value.HasValue);
            Assert.Null(value.Data);
        }

        [Fact]
        public async Task TestRedisRemove()
        {
            var services = InitRedisServiceCollection();

            var serviceProvider = services.BuildServiceProvider();
            var cache = serviceProvider.GetRequiredService<ICache>();
            cache.SetValueWithAbsolute("test", "test", TimeSpan.FromSeconds(3));
            await Task.Delay(2000);
            var value = cache.GetValue<string>("test");
            Assert.True(value.HasValue);
            Assert.Equal("test", value.Data);
            cache.Remove("test");
            value = cache.GetValue<string>("test");
            Assert.False(value.HasValue);
            Assert.Null(value.Data);
        }
    }
}