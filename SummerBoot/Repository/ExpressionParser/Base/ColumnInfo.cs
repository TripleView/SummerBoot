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
        /// �ж��Ƿ�ɿ�
        /// </summary>
        public bool IsNullable { get; set; }
        /// <summary>
        /// Ignore during update
        /// �����к���
        /// </summary>
        public bool IsIgnoreWhenUpdate { get; set; }
    }
}