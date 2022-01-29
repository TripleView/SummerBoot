using System;

namespace SummerBoot.Repository.Attributes
{
    /// <summary>
    /// 自定义sql查询的时候绑定where条件
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = true)]
    public class BindWhereAttribute : Attribute
    {

    }
}