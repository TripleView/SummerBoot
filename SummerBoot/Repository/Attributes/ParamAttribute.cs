using System;

namespace SummerBoot.Repository
{
    /// <summary>
    /// 参数别名注解
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, Inherited = true)]
    public class ParamAttribute:Attribute
    {
        public string Alias { get; }

        public ParamAttribute(string alias)
        {
            this.Alias = alias;
        }
    }
}