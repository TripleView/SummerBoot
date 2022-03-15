using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SummerBoot.Feign
{
    public interface IRequestInterceptor
    {
        Task ApplyAsync(RequestTemplate requestTemplate);
    }
}
