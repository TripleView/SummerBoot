using System.Collections.Generic;
using System.Reflection;
using SqlParser.Net.Ast.Expression;
using SummerBoot.Repository.Core;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public class DbQueryResult
    {
        public SqlExpression ExecuteSqlExpression { get; set; } 
        /// <summary>
        /// 执行的sql
        /// </summary>
        public string Sql
        {
            get => ExecuteSqlExpression.ToSql();
            set
            {

            }
        }
        /// <summary>
        /// 计算总数的sql
        /// </summary>
        public string CountSql { get; set; }

        public DynamicParameters DynamicParameters { get; set; }
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