using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SummerBoot.Feign
{
    [AttributeUsage(AttributeTargets.Method,Inherited = true,AllowMultiple = false)]
    public abstract class HttpMappingAttribute:Attribute
    {
        public string Value { get; }

        protected HttpMappingAttribute(string value)
        {
            Value = value;
        }
    }
}
