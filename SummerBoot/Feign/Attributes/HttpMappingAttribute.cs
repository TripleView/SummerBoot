using System;

namespace SummerBoot.Feign.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public abstract class HttpMappingAttribute : Attribute
    {
        public string Value { get; }
        /// <summary>
        /// 仅使用路径作为url
        /// use path as url
        /// </summary>
        public bool UsePathAsUrl { get; set; }

        protected HttpMappingAttribute(string value)
        {
            Value = value;
        }
    }
}
