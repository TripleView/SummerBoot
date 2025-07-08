using System.Collections.Generic;
using System.Reflection;
using SummerBoot.Repository.Core;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public class DbQueryResult
    {
        /// <summary>
        /// 执行的sql
        /// </summary>
        public string Sql { get; set; }
        /// <summary>
        /// 计算总数的sql
        /// </summary>
        public string CountSql { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public List<SqlParameter> SqlParameters { get; set; }
        /// <summary>
        /// 插入数据库后获取ID的sql
        /// </summary>
        public string LastInsertIdSql { get; set; }
        /// <summary>
        /// Id 字段的反射属性信息
        /// </summary>
        public PropertyInfo IdKeyPropertyInfo { get; set; }
        /// <summary>
        /// id字段的名称，有些数据库大小写敏感
        /// </summary>
        public string IdName { get; set; }
        /// <summary>
        /// 列字段的信息
        /// </summary>
        public List<DbQueryResultPropertyInfoMapping> PropertyInfoMappings { get; set; }
        /// <summary>
        /// Get dynamic parameters
        /// 获取动态参数
        /// </summary>
        /// <returns></returns>
        public DynamicParameters GetDynamicParameters()
        {
            if (SqlParameters == null || SqlParameters.Count == 0)
            {
                return null;
            }

            var result = new DynamicParameters();
            foreach (var parameter in SqlParameters)
            {
                result.Add(parameter.ParameterName, parameter.Value, valueType: parameter.ParameterType);
            }

            return result;
        }
    }

    public class DbQueryResultPropertyInfoMapping
    {
        /// <summary>
        /// 列名
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// 对应的属性
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }
    }
}