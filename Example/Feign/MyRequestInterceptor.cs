using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SummerBoot.Feign;

namespace Example.Feign
{
    public class MyRequestInterceptor : IRequestInterceptor
    {
        public void Apply(RequestTemplate requestTemplate)
        {
            requestTemplate.Headers.Add("testHeader",new List<string>(){"123"});
        }
    }
}
