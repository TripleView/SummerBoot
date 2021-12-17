using System.Collections.Generic;

namespace ExpressionParser.Parser
{
    public class PageQueryResult
    {
        /// <summary>
        /// 分页的sql
        /// </summary>
        public string PageSql { get; set; }
        /// <summary>
        /// 计算总量的sql
        /// </summary>
        public string TotalCountSql { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public List<SqlParameter> SqlParameters { get; set; }
       
    }
}