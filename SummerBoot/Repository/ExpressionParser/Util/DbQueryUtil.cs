using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using SummerBoot.Core;

namespace SummerBoot.Repository.ExpressionParser.Util
{
    public static class DbQueryUtil
    {
        /// <summary>
        /// Get table name
        /// ��ȡ����
        /// </summary>
        /// <param name="tableType"></param>
        /// <returns></returns>
        public static string GetTableName(Type tableType, string leftQualifiers = "", string rightQualifiers = "")
        {

            //����tableAttribute����,������û���Զ������
            var tableAttribute = tableType.GetCustomAttribute<TableAttribute>();
            //���û�и����ԣ�ֱ��ʹ��������Ϊ����
            var tableName = tableAttribute == null ? tableType.Name : tableAttribute.Name;
            var schemaName = tableAttribute == null ? "" : tableAttribute.Schema;
            var result = "";
            if (schemaName.HasText())
            {
                result += GetQualifiersName(schemaName) + ".";
            }

            result += GetQualifiersName(tableName);
            return result;
        }

        private static string GetQualifiersName(string name)
        {
            return  name ;
        }
        /// <summary>
        /// Get column names
        /// ��ȡ����
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
            return GetQualifiersName(columnName);
        }

    }
}