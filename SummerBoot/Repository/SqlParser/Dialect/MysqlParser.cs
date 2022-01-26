using System;
using System.Collections.Generic;
using SummerBoot.Repository.SqlParser.Dto;

namespace SummerBoot.Repository.SqlParser.Dialect
{
    public class MysqlParser : SqlParser
    {
        public MysqlParser() : base("@", "`", "`")
        {

        }

        public override List<string> KeyWordList()
        {
            BaseKeyWordList.Add("limit");

            return BaseKeyWordList;
        }

        public override ParserPageStringResult ParserPage(string sql, int page, int pageSize)
        {
            var result = new ParserPageStringResult()
            {
                SqlParameters = new List<SqlParameter>()
            };

            if (!sql.ToLower().Contains("select"))
            {
                throw new NotSupportedException("must be select sql");
            }

            var pageSkip = (page - 1) * pageSize;

            result.SqlParameters.Add(new SqlParameter() { ParameterName = PageSizeName, Value = pageSize, ParameterType = typeof(int) });
            result.SqlParameters.Add(new SqlParameter() { ParameterName = PageSkipName, Value = pageSkip, ParameterType = typeof(int) });

            result.PageSql = $"select * from ({sql}) sbOuter limit {BoxPageSkipName},{BoxPageSizeName}";
            result.CountSql = $"select count(1) from ({sql}) sbCount";

            return result;
        }

    }
}