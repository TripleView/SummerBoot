using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Core;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Xunit;

namespace SummerBoot.Test.Feign
{
    public class FeignTest
    {
        [Fact]
        public async Task TestGet()
        {
            var services = new ServiceCollection();
            services.TryAddTransient<HttpMessageHandlerBuilder, DefaultHttpMessageHandlerBuilder>();
            services.AddSingleton<IHttpClientFactory, TestFeignHttpClientFactory>();

            services.AddSummerBoot();
            services.AddSummerBootFeign();

            var serviceProvider = services.BuildServiceProvider();
            var testFeign = serviceProvider.GetRequiredService<ITestFeign>();
            var result = await testFeign.Test(new test() { Name = "hzp", Age = 10 });

        }
    }
}