using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SummerBoot.Feign
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ParamAttribute : Attribute
    {
        public string Value { get; }

        public ParamAttribute(string value = "")
        {
            Value = value;
        }
    }
}
