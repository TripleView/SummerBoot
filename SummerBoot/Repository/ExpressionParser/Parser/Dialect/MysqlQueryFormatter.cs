using SummerBoot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SummerBoot.Repository.ExpressionParser.Parser.Dialect
{
    public class MysqlQueryFormatter : QueryFormatter
    {
        public MysqlQueryFormatter(DatabaseUnit databaseUnit):base("@","`","`",databaseUnit)
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

        /// <summary>
        /// 多表联查
        /// </summary>
        /// <param name="select"></param>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private void HandlingMulTiQueryPaging(SelectExpression @select)
        {
            _sb.Append("SELECT ");

            if (!select.ColumnsPrefix.IsNullOrWhiteSpace())
            {
                _sb.AppendFormat("{0} ", select.ColumnsPrefix);
            }

            int index = 0;
            foreach (var column in select.Columns)
            {
                if (index++ > 0) _sb.Append(", ");
                this.VisitColumn(column);
            }

            var alias = select.Alias;

            if (select.From != null)
            {
                _sb.Append(" FROM ");
                //_sb.Append("(");
                if (select.From is TableExpression table)
                {
                    if (table.Schema.HasText())
                    {
                        _sb.Append(table.Schema + ".");
                    }
                    _sb.Append(BoxTableName(table.Name));
                    //_sb.AppendFormat(" {0}", select.Alias);
                    _sb.AppendFormat(" {0}", BoxTableName(select.Alias));
                }
                else if (select.From is SelectExpression subSelectExpression)
                {
                    _sb.Append("(");
                    this.VisitSelect(subSelectExpression);
                    _sb.AppendFormat(") {0}", BoxTableName(select.Alias));
                }

            }
            else
            {
                throw new ArgumentException("loss from");
            }

            if (select.Joins.Count > 0)
            {
                foreach (var joinExpression in select.Joins)
                {
                    _sb.Append($" {JoinTypeToString(joinExpression.JoinType)} {BoxTableName(joinExpression.JoinTable.Name)} {BoxTableName(joinExpression.JoinTableAlias)} on ");
                    this.VisitWhere(joinExpression.JoinCondition);
                }
            }

            var hasWhere = false;
            if (select.Where != null)
            {
                hasWhere = true;
                _sb.Append(" WHERE ");
                this.VisitWhere(select.Where);
            }

            //添加软删除过滤逻辑
            if (RepositoryOption.Instance != null && RepositoryOption.Instance.IsUseSoftDelete && select.From is TableExpression tablex && (
                    typeof(BaseEntity).IsAssignableFrom(tablex.Type) || typeof(OracleBaseEntity).IsAssignableFrom(tablex.Type)))
            {
                var softDeleteColumn = tablex.Columns.FirstOrDefault(it => it.ColumnName.ToLower() == "active");
                if (softDeleteColumn != null)
                {
                    var softDeleteParameterName = BoxParameter(1, typeof(int));
                    if (!hasWhere)
                    {
                        _sb.Append(" WHERE ");
                    }
                    else
                    {
                        _sb.Append(" and ");
                    }
                    _sb.Append($" {BoxColumnName(softDeleteColumn.ColumnName)}={softDeleteParameterName}");
                }
            }

            if (select.GroupBy.IsNotNullAndNotEmpty())
            {
                _sb.Append(" GROUP BY ");
                for (var i = 0; i < select.GroupBy.Count; i++)
                {
                    var groupBy = select.GroupBy[i];
                    this.VisitColumn(groupBy.ColumnExpression);
                    if (i < select.GroupBy.Count - 1)
                    {
                        _sb.Append(",");
                    }
                }
            }

            if (select.OrderBy.IsNotNullAndNotEmpty() && !select.IsIgnoreOrderBy)
            {
                _sb.Append(" ORDER BY ");
                for (var i = 0; i < select.OrderBy.Count; i++)
                {
                    var orderBy = select.OrderBy[i];
                    this.VisitColumn(orderBy.ColumnExpression);
                    _sb.Append(orderBy.OrderByType == OrderByType.Desc ? " DESC" : "");
                    if (i < select.OrderBy.Count - 1)
                    {
                        _sb.Append(",");
                    }
                }
            }
        }


        protected override void HandlingPaging(SelectExpression select)
        {
            if (select.Joins.Any())
            {
                HandlingMulTiQueryPaging(select);
            }
            else
            {
                base.HandlingNormal(select);
            }
            
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
                _sb.Append(BoxParameter(select.Skip.Value,typeof(int)));
            }
            else
            {
                _sb.Append(BoxParameter(0, typeof(int)));
            }

            _sb.Append(",");
            var hasTake = select.Take.HasValue;
            if (hasTake)
            {
                _sb.Append(BoxParameter(select.Take.Value, typeof(int)));
            }
            else
            {
                _sb.Append(BoxParameter(int.MaxValue, typeof(int)));
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