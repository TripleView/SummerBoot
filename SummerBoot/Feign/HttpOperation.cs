using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SummerBoot.Feign
{
    public abstract class HttpOperation
    {
        public string Value { get; }
        public HttpMethod HttpMethod { get; }
        protected HttpOperation(string value,HttpMethod httpMethod)
        {
            Value = value;
            HttpMethod = httpMethod;
        }
    }
}
