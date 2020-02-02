using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SummerBoot.Feign
{
    public class HttpGetOperation:HttpOperation
    {
        public HttpGetOperation(string value):base(value,HttpMethod.Get)
        {

        }
    }
}
