using System;
using System.Collections.Generic;
using SqlParser.Dto;

namespace SqlParser.Dialect
{
    public class SqliteParser : SqlParser
    {
        public SqliteParser():base(":", "`", "`")
        {
            
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

            result.PageSql = $"select * from ({sql}) sbOuter limit {BoxPageSizeName} offset {BoxPageSkipName}";
            result.CountSql = $"select count(1) from ({sql}) sbCount";

            return result;
        }
    }
}