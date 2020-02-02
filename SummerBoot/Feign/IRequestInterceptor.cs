using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SummerBoot.Feign
{
    public interface IRequestInterceptor
    {
        void Apply(RequestTemplate requestTemplate);
    }
}
