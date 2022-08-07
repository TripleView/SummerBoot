using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SummerBoot.Feign;

namespace Example.Feign
{
    public class MyRequestInterceptor : IRequestInterceptor
    {
        public MyRequestInterceptor(ITestFeign testFeign)
        {
            this.testFeign = testFeign;
        }
        private readonly ITestFeign testFeign;

        public async Task ApplyAsync(RequestTemplate requestTemplate)
        {
            requestTemplate.Headers.Add("testHeader", new List<string>() { "123" });
     
            await Task.CompletedTask;
        }
    }
}
