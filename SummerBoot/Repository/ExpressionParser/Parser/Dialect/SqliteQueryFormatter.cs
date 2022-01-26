namespace SummerBoot.Repository.ExpressionParser.Parser.Dialect
{
    public class SqliteQueryFormatter : QueryFormatter
    {
        public SqliteQueryFormatter():base(":", "`","`")
        {
            
        }

        protected override string GetLastInsertIdSql()
        {
            return "SELECT last_insert_rowid() id";
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
                _sb.Append(BoxParameter(select.Take.Value));
            }
            else
            {
                _sb.Append(BoxParameter(int.MaxValue));
            }
            
            _sb.Append(" offset ");

            var hasSkip = select.Skip.HasValue;
            if (hasSkip)
            {
                _sb.Append(BoxParameter(select.Skip.Value));
            }
            else
            {
                _sb.Append(BoxParameter(0));
            }
        }
    }
}