using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SummerBoot.Repository.ExpressionParser.Parser.Dialect
{
    public class PgsqlQueryFormatter : QueryFormatter
    {
        public PgsqlQueryFormatter(DatabaseUnit databaseUnit):base("@", "\"","\"",databaseUnit)
        {
            
        }

        public override DbQueryResult Insert<T>(T insertEntity)
        {
            var result = base.Insert(insertEntity);
            if (result.IdKeyPropertyInfo != null)
            {

                result.Sql += $" RETURNING {BoxColumnName(result.IdName)}";
            }

            return result;
        }

        protected override string GetFunctionAlias(string functionName)
        {
            if (functionName == "LEN")
            {
                return "LENGTH";
            }
            return base.GetFunctionAlias(functionName);
        }

        protected override void HandlingPaging(SelectExpression select)
        {
            base.HandlingNormal(select);
            countSqlSb.Append($"select count(1) from ({_sb}) sbCount");
            BoxPagination(select);
        }

        protected void BoxPagination(SelectExpression select)
        {
            if (!select.Skip.HasValue && !select.Take.HasValue)
            {
                return;
            }
            _sb.Append(" LIMIT ");
            var hasTake = select.Take.HasValue;
            if (hasTake)
            {
                _sb.Append(BoxParameter(select.Take.Value, typeof(int)));
            }
            else
            {
                _sb.Append(BoxParameter(int.MaxValue, typeof(int)));
            }

            _sb.Append(" offset ");

            var hasSkip = select.Skip.HasValue;
            if (hasSkip)
            {
                _sb.Append(BoxParameter(select.Skip.Value,typeof(int)));
            }
            else
            {
                _sb.Append(BoxParameter(0, typeof(int)));
            }
        }

    }
}