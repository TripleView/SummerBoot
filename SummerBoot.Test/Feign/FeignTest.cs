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
using System.Collections.Specialized;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace SummerBoot.Test.Feign
{
    public interface t1
    {

    }

    public class tt1 : t1
    {

    }

    public interface t2:t1
    {

    }

    public class tt2 : t2
    {

    }
    public class FeignTest
    {

        [Fact]
        public async Task Test()
        {

            t1 d = new tt1();
            t2 d2= new tt2();
            var c = d is t1;
            var c1 = d2 is t1;
        }

        /// <summary>
        /// 测试仅使用path作为整体url
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task TestUsePathAsUrl()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();

            await testFeign.TestUsePathAsUrl(new Test() { Name = "sb", Age = 3 });

        }


        [Fact]
        public async Task TestReturnTask()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();

            await testFeign.TestReturnTask(new Test() { Name = "sb", Age = 3 });
        
        }

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

            Assert.Equal("No matching mock handler for \"POST http://localhost:5001/home/testIgnoreInterceptor\"", exception.Message);
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

        [Fact]
        public async Task TestBasicAuthorization()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();

            var result = await testFeign.TestBasicAuthorization(new BasicAuthorization("abc","123"));
            Assert.Equal("sb", result.Name);
            Assert.Equal(3, result.Age);
        }
        

        [Fact]
        public async Task TestHeadersWithInterfaceAndMethod()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeignWithHeader>();

            var result = await testFeign.TestHeadersWithInterfaceAndMethod(new Test() { Name = "sb", Age = 3 });
            Assert.Equal("sb", result.Name);
            Assert.Equal(3, result.Age);
        }

        [Fact]
        public async Task TestHeadersWithInterface()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeignWithHeader>();

            var result = await testFeign.TestHeadersWithInterface(new Test() { Name = "sb", Age = 3 });
            Assert.Equal("sb", result.Name);
            Assert.Equal(3, result.Age);
        }

        [Fact]
        public async Task TestOriginResponse()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();

            var result = await testFeign.TestOriginResponse(new Test() { Name = "sb", Age = 3 });

            var resultContent =await result.Content.ReadAsStringAsync();
            Assert.Equal("{\"Name\": \"sb\",\"Age\": 3}", resultContent);
        }

        [Fact]
        public async Task TestDownLoadWithStream()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();

            var result = await testFeign.TestDownLoadWithStream();

            var resultContent = result.ConvertToString();
            Assert.Equal("456", resultContent);
        }

        /// <summary>
        /// 测试参数内嵌
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task TestEmbedded()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();
            var result = await testFeign.TestEmbedded(new EmbeddedTest3()
            {
                Name = "sb",
                Test = new EmbeddedTest2()
                {
                    Age = 3
                }
            });
            Assert.Equal("ok", result);
           
        }

        /// <summary>
        /// 测试参数内嵌
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task TestNotEmbedded()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();
            var result = await testFeign.TestNotEmbedded(new EmbeddedTest()
            {
                Name = "sb",
                Test = new EmbeddedTest2()
                {
                    Age = 3
                }
            });
            Assert.Equal("ok", result);
        }

        static readonly string CONFIG_FILE = "app.json";  // 配置文件地址
        /// <summary>
        /// 测试从配置中读取url
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task TestGetUrlFromConfiguration()
        {
            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());  // 获取当前程序执行目录
            build.AddJsonFile(CONFIG_FILE, true, true);
            var configurationRoot = build.Build();
            
            var services = new ServiceCollection();
            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();
            services.AddSingleton<IConfiguration>(configurationRoot);
            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();
            var testFeign = serviceProvider.GetRequiredService<ITestFeignWithConfiguration>();

            var result = await testFeign.TestQuery(new Test() { Name = "sb", Age = 3 });
            Assert.Equal("sb", result.Name);
            Assert.Equal(3, result.Age);
        }
    }
}