using System;

namespace SummerBoot.Feign.Attributes
{
    /// <summary>
    /// 参数是否嵌入
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter|AttributeTargets.Property)]
    public class EmbeddedAttribute : Attribute
    {
    }
}
