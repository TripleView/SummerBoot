using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SummerBoot.Feign
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HeadersAttribute:Attribute
    {
        public string[] Param { get; }
        public HeadersAttribute(params  string[] param)
        {
            Param = param;
        }
    }
}
