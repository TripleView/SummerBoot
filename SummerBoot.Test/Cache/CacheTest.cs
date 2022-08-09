﻿using System;
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
        private ServiceProvider InitMemoryCacheServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddSummerBoot();
            services.AddSummerBootCache(it => it.UseMemory());

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
        [Fact]
        public async Task TestNoneValue()
        {
            var serviceProvider = InitMemoryCacheServiceProvider();
            var cache = serviceProvider.GetRequiredService<ICache>();

            var value = cache.GetValue<string>("test");
            Assert.False(value.HasValue);
            Assert.Null(value.Data);
        }

        [Fact]
        public async Task TestSetValueWithAbsolute()
        {
            var serviceProvider = InitMemoryCacheServiceProvider();
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
            var serviceProvider = InitMemoryCacheServiceProvider();
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
            var serviceProvider = InitMemoryCacheServiceProvider();
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

        [Fact]
        public async Task TestNoneValueAsync()
        {
            var serviceProvider = InitMemoryCacheServiceProvider();
            var cache = serviceProvider.GetRequiredService<ICache>();

            var value = await cache.GetValueAsync<string>("test");
            Assert.False(value.HasValue);
            Assert.Null(value.Data);
        }

        [Fact]
        public async Task TestSetValueWithAbsoluteAsync()
        {
            var serviceProvider = InitMemoryCacheServiceProvider();
            var cache = serviceProvider.GetRequiredService<ICache>();
            cache.SetValueWithAbsolute("test", "test", TimeSpan.FromSeconds(3));
            await Task.Delay(2000);
            var value = await cache.GetValueAsync<string>("test");
            Assert.True(value.HasValue);
            Assert.Equal("test", value.Data);
            await Task.Delay(1000);
            value = await cache.GetValueAsync<string>("test");
            Assert.False(value.HasValue);
            Assert.Null(value.Data);
        }

        [Fact]
        public async Task TestSetValueWithSlidingAsync()
        {
            var serviceProvider = InitMemoryCacheServiceProvider();
            var cache = serviceProvider.GetRequiredService<ICache>();
            await cache.SetValueWithSlidingAsync("test", "test", TimeSpan.FromSeconds(3));
            await Task.Delay(2000);
            var value = await cache.GetValueAsync<string>("test");
            Assert.True(value.HasValue);
            Assert.Equal("test", value.Data);
            await Task.Delay(3000);
            value = await cache.GetValueAsync<string>("test");
            Assert.False(value.HasValue);
            Assert.Null(value.Data);
        }

        [Fact]
        public async Task TestRemoveAsync()
        {
            var serviceProvider = InitMemoryCacheServiceProvider();
            var cache = serviceProvider.GetRequiredService<ICache>();
            await cache.SetValueWithAbsoluteAsync("test", "test", TimeSpan.FromSeconds(3));
            await Task.Delay(2000);
            var value = await cache.GetValueAsync<string>("test");
            Assert.True(value.HasValue);
            Assert.Equal("test", value.Data);
            cache.Remove("test");
            value = await cache.GetValueAsync<string>("test");
            Assert.False(value.HasValue);
            Assert.Null(value.Data);
        }

        private ServiceProvider InitRedisServiceCollection()
        {
            var connectionString = MyConfiguration.GetConfiguration("redisConnectionString");
            var services = new ServiceCollection();

            services.AddSummerBoot();
            services.AddSummerBootCache(it =>
            {
                it.UseRedis(connectionString);
            });

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }

        [Fact]
        public async Task TestRedisNoneValue()
        {

            var serviceProvider = InitMemoryCacheServiceProvider();
            var cache = serviceProvider.GetRequiredService<ICache>();

            var value = cache.GetValue<string>("test");
            Assert.False(value.HasValue);
            Assert.Null(value.Data);
        }

        [Fact]
        public async Task TestRedisSetValueWithAbsolute()
        {
            var serviceProvider = InitMemoryCacheServiceProvider();
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
            var serviceProvider = InitMemoryCacheServiceProvider();
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
            var serviceProvider = InitMemoryCacheServiceProvider();
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

        [Fact]
        public async Task TestRedisNoneValueAsync()
        {

            var serviceProvider = InitMemoryCacheServiceProvider();
            var cache = serviceProvider.GetRequiredService<ICache>();

            var value = await cache.GetValueAsync<string>("test");
            Assert.False(value.HasValue);
            Assert.Null(value.Data);
        }

        [Fact]
        public async Task TestRedisSetValueWithAbsoluteAsync()
        {
            var serviceProvider = InitMemoryCacheServiceProvider();
            var cache = serviceProvider.GetRequiredService<ICache>();
            await cache.SetValueWithAbsoluteAsync("test", "test", TimeSpan.FromSeconds(3));
            await Task.Delay(2000);
            var value = await cache.GetValueAsync<string>("test");
            Assert.True(value.HasValue);
            Assert.Equal("test", value.Data);
            await Task.Delay(1000);
            value = await cache.GetValueAsync<string>("test");
            Assert.False(value.HasValue);
            Assert.Null(value.Data);
        }

        [Fact]
        public async Task TestRedisSetValueWithSlidingAsync()
        {
            var serviceProvider = InitMemoryCacheServiceProvider();
            var cache = serviceProvider.GetRequiredService<ICache>();
            await cache.SetValueWithSlidingAsync("test", "test", TimeSpan.FromSeconds(3));
            await Task.Delay(2000);
            var value = await cache.GetValueAsync<string>("test");
            Assert.True(value.HasValue);
            Assert.Equal("test", value.Data);
            await Task.Delay(3000);
            value = await cache.GetValueAsync<string>("test");
            Assert.False(value.HasValue);
            Assert.Null(value.Data);
        }

        [Fact]
        public async Task TestRedisRemoveAsync()
        {
            var serviceProvider = InitMemoryCacheServiceProvider();
            var cache = serviceProvider.GetRequiredService<ICache>();
            await cache.SetValueWithAbsoluteAsync("test", "test", TimeSpan.FromSeconds(3));
            await Task.Delay(2000);
            var value = await cache.GetValueAsync<string>("test");
            Assert.True(value.HasValue);
            Assert.Equal("test", value.Data);
            await cache.RemoveAsync("test");
            value = cache.GetValue<string>("test");
            Assert.False(value.HasValue);
            Assert.Null(value.Data);
        }
    }
}