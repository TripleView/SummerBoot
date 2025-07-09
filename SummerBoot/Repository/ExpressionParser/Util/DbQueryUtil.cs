using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace SummerBoot.Repository.ExpressionParser.Util
{
    public static class DbQueryUtil
    {
        /// <summary>
        /// Get table name
        /// 获取表名
        /// </summary>
        /// <param name="tableType"></param>
        /// <returns></returns>
        public static string GetTableName(Type tableType)
        {
            //查找tableAttribute特性,看下有没有自定义表名
            var tableAttribute = tableType.GetCustomAttribute<TableAttribute>();
            //如果没有该特性，直接使用类名作为表名
            var tableName = tableAttribute == null ? tableType.Name : tableAttribute.Name;
            return tableName;
        }
        /// <summary>
        /// Get column names
        /// 获取列名
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