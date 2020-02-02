using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SummerBoot.Feign
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class HeaderAttribute:Attribute
    {
        public string Value { get; }

        public HeaderAttribute(string value)
        {
            Value = value;
        }
    }
}
