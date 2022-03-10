using System;

namespace SummerBoot.Feign.Attributes
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
