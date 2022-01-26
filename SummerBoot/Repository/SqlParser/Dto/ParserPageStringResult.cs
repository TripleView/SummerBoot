using System.Collections.Generic;

namespace SummerBoot.Repository.SqlParser.Dto
{
    /// <summary>
    /// 解析分页的结果
    /// </summary>
    public class ParserPageStringResult
    {
        /// <summary>
        /// 分页语句
        /// </summary>
        public string PageSql { get; set; }
        /// <summary>
        /// 计算总量的语句
        /// </summary>
        public string CountSql { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public List<SqlParameter> SqlParameters { get; set; }   
    }
}