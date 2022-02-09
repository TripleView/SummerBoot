using System;

namespace SummerBoot.Repository.Attributes
{
    /// <summary>
    /// 自定义sql查询的时候绑定where条件
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter|AttributeTargets.Property, Inherited = true)]
    public class BindWhereAttribute : Attribute
    {
        public string ParameterName { get; set; }

        public BindWhereAttribute(string parameterName="")
        {
            ParameterName = parameterName;
        }
    }
}