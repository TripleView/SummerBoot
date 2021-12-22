using System;
using System.Collections.Generic;
using System.Data;
using SqlParser.Dto;

namespace SqlParser.Dialect
{
    public class OracleParser : SqlParser
    {
        public OracleParser():base(":", "`", "`")
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
            var sqlOrderBy = GetOrderByClause(sql);
            var sqlOrderByRemoved = sql;

            if (string.IsNullOrEmpty(sqlOrderBy))
            {
                sqlOrderBy = $"order by null ";
            }
            else
            {
                var newSqlOrderByRemoved = GetSelectFieldsClause(sql);

                var newSqlOrderByRemovedArr = newSqlOrderByRemoved.Split(',');

                var newSqlOrderBy = "order by ";
                var orderByFields = new List<string>();
                //order by多个字段
                if (sqlOrderBy.IndexOf(",") > -1)
                {
                    var orderBySplitArr = sqlOrderBy.Split(',');
                    foreach (var s in orderBySplitArr)
                    {
                        GetOrderByFields(s, newSqlOrderByRemovedArr, orderByFields);
                    }
                }
                else
                {
                    GetOrderByFields(sqlOrderBy, newSqlOrderByRemovedArr, orderByFields);
                }

                newSqlOrderBy += string.Join(",", orderByFields);
                sqlOrderBy = newSqlOrderBy;
            }

            result.SqlParameters.Add(new SqlParameter(){ ParameterName = PageSizeName,Value = pageSize,ParameterType = typeof(int) } );
            result.SqlParameters.Add(new SqlParameter() { ParameterName = PageSkipName, Value = pageSkip, ParameterType = typeof(int) });

            var columnsOnly = $"sbInner.* FROM ({sqlOrderByRemoved}) sbInner";

            result.PageSql = $"select * from (select row_number() over ({sqlOrderBy}) pageNo, {columnsOnly}) sbOuter where pageNo > {BoxPageSizeName} and pageNo <= {BoxPageSkipName} + {BoxPageSizeName}";
            result.CountSql = $"select count(1) from ({sql}) sbCount";

            return result;

        }
    }
}