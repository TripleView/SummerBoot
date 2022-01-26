using System.Reflection;

namespace SummerBoot.Repository.ExpressionParser.Base
{
    public class ColumnInfo
    {
        /// <summary>
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// </summary>
        public bool IsKey { get; set; }
        /// <summary>
        /// </summary>
        public PropertyInfo Property { get; set; }
        /// <summary>
        /// 判断是否可空
        /// </summary>
        public bool IsNullable { get; set; }
    }
}