using System.Collections.Generic;
using System.Reflection;

namespace ExpressionParser.Parser
{
    public class DbQueryResult
    {
        /// <summary>
        /// 执行的sql
        /// </summary>
        public string Sql { get; set; }
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
    }
}