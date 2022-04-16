using System;

namespace SummerBoot.Repository.Attributes
{
    /// <summary>
    /// 自定义Decimal类型的精度属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class DecimalPrecisionAttribute : Attribute
    {
        /// <summary>
        /// 自定义Decimal类型的精度属性
        /// </summary>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        public DecimalPrecisionAttribute(int precision, int scale)
        {
            Precision = precision;
            Scale = scale;
        }
        /// <summary>
        /// 精确度（默认18）
        /// </summary>
        public int Precision { get; set; }
        /// <summary>
        /// 保留位数（默认2）
        /// </summary>
        public int Scale { get; set; }
      
    }
}