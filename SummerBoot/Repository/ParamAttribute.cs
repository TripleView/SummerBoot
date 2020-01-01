using System;

namespace SummerBoot.Repository
{
    /// <summary>
    /// 参数别名注解
    /// </summary>
    public class ParamAttribute:Attribute
    {
        public string Alias { get; }

        public ParamAttribute(string alias)
        {
            this.Alias = alias;
        }
    }
}