using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SummerBoot.Feign
{
    public class HttpPostOperation : HttpOperation
    {
        public HttpPostOperation(string value):base(value,HttpMethod.Post)
        {

        }
    }
}
