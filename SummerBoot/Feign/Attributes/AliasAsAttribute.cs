using System;

namespace SummerBoot.Feign.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter|AttributeTargets.Property)]
    public class AliasAsAttribute : Attribute
    {
        public string Name { get; set; }
    }
}