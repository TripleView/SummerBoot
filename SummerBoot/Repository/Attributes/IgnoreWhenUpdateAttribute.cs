using System;

namespace SummerBoot.Repository.Attributes
{
    /// <summary>
    /// Ignore this column during update
    /// 更新时忽略该列
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class IgnoreWhenUpdateAttribute : Attribute
    {

    }
}