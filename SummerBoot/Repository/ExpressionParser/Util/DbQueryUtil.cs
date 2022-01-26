using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace SummerBoot.Repository.ExpressionParser.Util
{
    public static class DbQueryUtil
    {
        /// <summary>
        /// 获取列明
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static string GetColumnName(MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                return "*";
            }
            var columnAttribute = memberInfo.GetCustomAttribute<ColumnAttribute>();

            var columnName = columnAttribute?.Name ?? memberInfo.Name;
            return columnName;
        }
        
    }
}