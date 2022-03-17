using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Core;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using SummerBoot.Feign;
using Xunit;

namespace SummerBoot.Test.Feign
{
    public class FeignTest
    {
        

        [Fact]
        public async Task TestQueryWithEscapeData()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();

            var result = await testFeign.TestQueryWithEscapeData(new Test() { Name = "哈哈哈", Age = 3 });
            Assert.Equal("哈哈哈", result.Name);
            Assert.Equal(3, result.Age);
        }

        [Fact]
        public async Task TestQueryWithExistCondition()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();

            var result = await testFeign.TestQueryWithExistCondition("sb");
            Assert.Equal("sb", result.Name);
            Assert.Equal(3, result.Age);
        }

        [Fact]
        public async Task TestQuery()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();
           
            var result = await testFeign.TestQuery(new Test() { Name = "sb", Age = 3 });
            Assert.Equal("sb", result.Name);
            Assert.Equal(3, result.Age);
        }

        [Fact]
        public async Task MultipartUploadFileWithByteArrayTest()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();
            var basePath = Path.Combine(AppContext.BaseDirectory, "123.txt");

            var byteArray=   File.ReadAllBytes(basePath);
           
            var result = await testFeign.MultipartTest(new Test() { Name = "sb", Age = 3 }, new MultipartItem(byteArray, "file", "123.txt"));
            Assert.Equal("sb", result.Name);
            Assert.Equal(3, result.Age);
        }

        [Fact]
        public async Task MultipartUploadFileWithStreamTest()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();
            var basePath = Path.Combine(AppContext.BaseDirectory, "123.txt");

            var fileStream= new FileInfo(basePath).OpenRead();
            var result = await testFeign.MultipartTest(new Test() { Name = "sb", Age = 3 }, new MultipartItem(fileStream, "file", "123.txt"));
            Assert.Equal("sb", result.Name);
            Assert.Equal(3, result.Age);
        }

        [Fact]
        public async Task MultipartUploadFileWithFileInfoTest()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();
            var basePath =Path.Combine(AppContext.BaseDirectory,"123.txt");
            var result = await testFeign.MultipartTest(new Test() { Name = "sb", Age = 3 },new MultipartItem(new FileInfo(basePath),"file","123.txt"));
            Assert.Equal("sb", result.Name);
            Assert.Equal(3, result.Age);
        }

        [Fact]
        public async Task TestUrlError()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();
            var result = await testFeign.TestUrlError(new Test() { Name = "sb", Age = 3 });
            Assert.Equal("sb", result.Name);
            Assert.Equal(3, result.Age);
        }

        [Fact]
        public async Task TestInterceptor()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();
            var result = await testFeign.TestInterceptor(new Test() { Name = "sb", Age = 3 });
            Assert.Equal("sb", result.Name);
            Assert.Equal(3, result.Age);
        }
        

        [Fact]
        public async Task TestIgnoreInterceptor()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();
            var exception =await Assert.ThrowsAsync<HttpRequestException>(async () =>
                await testFeign.TestIgnoreInterceptor(new Test() { Name = "sb", Age = 3 })
                );

            Assert.Equal("Response status code does not indicate success: 404 (No matching mock handler for \"POST http://localhost:5001/home/testIgnoreInterceptor\").", exception.Message);
        }

        [Fact]
        public async Task TestForm()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();
            var result = await testFeign.TestForm(new Test() { Name = "sb", Age = 3 });
            Assert.Equal("sb",result.Name);
            Assert.Equal(3,result.Age);
        }

        [Fact]
        public async Task TestJson()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();
            var result = await testFeign.TestJson(new Test() { Name = "sb", Age = 3 });
            Assert.Equal("sb", result.Name);
            Assert.Equal(3, result.Age);
        }

        [Fact]
        public async Task TestReplaceVariableInUrlWithClassWithAlias()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();
            var result = await testFeign.TestReplaceVariableInUrlWithClassWithAlias(new Test() { Name = "sb", Age = 3 },new VariableClass(){Name = "form"});
            Assert.Equal("sb", result.Name);
            Assert.Equal(3, result.Age);
        }

        [Fact]
        public async Task TestReplaceVariableInUrlWithClass()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();
            var result = await testFeign.TestReplaceVariableInUrlWithClass(new Test() { Name = "sb", Age = 3 }, new VariableClass2() { methodName = "form" });
            Assert.Equal("sb", result.Name);
            Assert.Equal(3, result.Age);
        }

        [Fact]
        public async Task TestReplaceVariableHasSpace()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();
            var result = await testFeign.TestReplaceVariableHasSpace(new Test() { Name = "sb", Age = 3 }, "form");
            Assert.Equal("sb", result.Name);
            Assert.Equal(3, result.Age);
        }

        [Fact]
        public async Task TestReplaceVariableInUrlWithAlias()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();
            var result = await testFeign.TestReplaceVariableInUrlWithAlias(new Test() { Name = "sb", Age = 3 }, "form");
            Assert.Equal("sb", result.Name);
            Assert.Equal(3, result.Age);
        }

        [Fact]
        public async Task TestReplaceVariableInUrl()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();
            var result = await testFeign.TestReplaceVariableInUrl(new Test() { Name = "sb", Age = 3 }, "form");
            Assert.Equal("sb", result.Name);
            Assert.Equal(3, result.Age);
        }

        [Fact]
        public async Task TestReplaceVariableInHeaderWithClassWithAlias()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();
            var result = await testFeign.TestReplaceVariableInHeaderWithClassWithAlias(new Test() { Name = "sb", Age = 3 }, new VariableClass() { Name = "a" });
            Assert.Equal("sb", result.Name);
            Assert.Equal(3, result.Age);
        }


        [Fact]
        public async Task TestReplaceVariableInHeaderWithClass()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();
            var result = await testFeign.TestReplaceVariableInHeaderWithClass(new Test() { Name = "sb", Age = 3 }, new VariableClass2() { methodName = "a" });
            Assert.Equal("sb", result.Name);
            Assert.Equal(3, result.Age);
        }

        [Fact]
        public async Task TestReplaceVariableInHeaderWithAlias()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();
            var result = await testFeign.TestReplaceVariableInHeaderWithAlias(new Test() { Name = "sb", Age = 3 },  "a" );
            Assert.Equal("sb", result.Name);
            Assert.Equal(3, result.Age);
        }

        [Fact]
        public async Task TestReplaceVariableInHeader()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();
            var result = await testFeign.TestReplaceVariableInHeader(new Test() { Name = "sb", Age = 3 }, "a");
            Assert.Equal("sb", result.Name);
            Assert.Equal(3, result.Age);
        }

        [Fact]
        public async Task TestHeaderCollection()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();

            var headerCollection = new HeaderCollection()
                { new KeyValuePair<string, string>("a", "a"), 
                    new KeyValuePair<string, string>("b", "b") };

            var result = await testFeign.TestHeaderCollection(new Test() { Name = "sb", Age = 3 }, headerCollection);
            Assert.Equal("sb", result.Name);
            Assert.Equal(3, result.Age);
        }

      
    }
}