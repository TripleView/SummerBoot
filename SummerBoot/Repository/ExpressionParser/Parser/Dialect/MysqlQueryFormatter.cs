using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SummerBoot.Repository.ExpressionParser.Parser.Dialect
{
    public class MysqlQueryFormatter : QueryFormatter
    {
        public MysqlQueryFormatter():base("@","`","`")
        {
            
        }


        protected override string GetLastInsertIdSql()
        {
            return "Select LAST_INSERT_ID() id";
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
            var hasSkip = select.Skip.HasValue;
            if (hasSkip)
            {
                _sb.Append(BoxParameter(select.Skip.Value));
            }
            else
            {
                _sb.Append(BoxParameter(0));
            }

            _sb.Append(",");
            var hasTake = select.Take.HasValue;
            if (hasTake)
            {
                _sb.Append(BoxParameter(select.Take.Value));
            }
            else
            {
                _sb.Append(BoxParameter(int.MaxValue));
            }

        }

        public override DbQueryResult FastBatchInsert<T>(List<T> insertEntitys)
        {
            Clear();
            var table = this.getTableExpression(typeof(T));
            var tableName = GetSchemaTableName(table.Schema, table.Name);

            var result = new DbQueryResult()
            {
                Sql = tableName,
                SqlParameters = this.sqlParameters,
                PropertyInfoMappings = table.Columns.Where(it => !(it.IsKey && it.IsDatabaseGeneratedIdentity)).Select(it => new DbQueryResultPropertyInfoMapping() { ColumnName = it.ColumnName, PropertyInfo = it.MemberInfo as PropertyInfo }).ToList()
            };

            return result;
        }
    }
}