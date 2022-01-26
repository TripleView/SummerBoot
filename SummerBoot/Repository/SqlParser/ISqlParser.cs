using SummerBoot.Repository.SqlParser.Dto;

namespace SummerBoot.Repository.SqlParser
{
    public interface ISqlParser
    {
        /// <summary>
        /// 解析分页语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        ParserPageStringResult ParserPage(string sql,int page,int pageSize);
    }
}