﻿using SummerBoot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SummerBoot.Repository.ExpressionParser.Parser.Dialect
{
    public class SqlServerQueryFormatter : QueryFormatter
    {
        public SqlServerQueryFormatter(DatabaseUnit databaseUnit) : base("@", "[", "]",databaseUnit)
        {

        }

        protected override string GetLastInsertIdSql()
        {
            return "select SCOPE_IDENTITY() id";
        }

        public override DbQueryResult Insert<T>(T insertEntity)
        {
            Clear();
            var table = this.getTableExpression(typeof(T));
            var tableName = GetSchemaTableName(table.Schema, table.Name);

            var parameterNameList = new List<string>();
            var columnNameList = new List<string>();

            var keyColumn = table.Columns.FirstOrDefault(it => it.IsKey && it.IsDatabaseGeneratedIdentity && it.ColumnName.ToLower().Contains("id") && it.MemberInfo is PropertyInfo);

            foreach (var column in table.Columns)
            {
                if (keyColumn != null && keyColumn == column)
                {
                    continue;
                }
                var columnName = BoxColumnName(column.ColumnName);
                columnNameList.Add(columnName);
                var parameterName = this.parameterPrefix + column.ColumnName;
                parameterNameList.Add(parameterName);
            }

            _sb.Append($"insert into {tableName} ({string.Join(",", columnNameList)}) values ({string.Join(",", parameterNameList)})");

            var result = new DbQueryResult()
            {
                Sql = this._sb.ToString().Trim(),
                SqlParameters = this.sqlParameters
            };


            if (keyColumn != null)
            {
                result.LastInsertIdSql = GetLastInsertIdSql();
                result.IdKeyPropertyInfo = keyColumn.MemberInfo as PropertyInfo;
                result.IdName = keyColumn.ColumnName;
            }

            return result;
        }
        /// <summary>
        /// 有跳过的分页
        /// </summary>
        /// <param name="select"></param>
        private void HasSkipPaging(SelectExpression select)
        {
            if (select.Joins.Any())
            {
                this.HasMultiQuerySkipPaging(select);
                return;
            }
            var externalAlias = this.GetNewAlias();

            _sb.Append("SELECT ");
            var isFromTable = false;

            if (!select.ColumnsPrefix.IsNullOrWhiteSpace())
            {
                _sb.AppendFormat("{0} ", select.ColumnsPrefix);
            }

            int index = 0;

            foreach (var column in select.Columns)
            {
                var tempColumn = column.DeepClone();

                if (index++ > 0) _sb.Append(", ");
                tempColumn.TableAlias = externalAlias;
                this.VisitColumn(tempColumn);
            }

            _sb.Append(" FROM (");
            _sb.Append("SELECT ");

            index = 0;
            if (select.From is TableExpression fromTable)
            {
                isFromTable = true;

                var except = fromTable.Columns.Where(it =>
                    select.Columns.Any(x => x.MemberInfo == it.MemberInfo) || select.OrderBy
                        .Select(x => x.ColumnExpression).Any(x => x.MemberInfo == it.MemberInfo)).Distinct().ToList();

                foreach (var column in except)
                {
                    if (index++ > 0) _sb.Append(", ");
                    column.TableAlias = select.Alias;

                    this.VisitColumn(column);
                }
            }
            else if (select.From is SelectExpression subSelectExpression && subSelectExpression.From is TableExpression fromTable2)
            {
                var except = fromTable2.Columns.Where(it =>
                    subSelectExpression.Columns.Any(x => x.MemberInfo == it.MemberInfo) || subSelectExpression.OrderBy
                        .Select(x => x.ColumnExpression).Any(x => x.MemberInfo == it.MemberInfo)).Distinct().ToList();

                foreach (var column in except)
                {
                    if (index++ > 0) _sb.Append(", ");
                    column.TableAlias = select.Alias;
                    this.VisitColumn(column);
                }
            }
            else
            {
                throw new NotSupportedException(nameof(select.From));
            }

            //提取orderBy
            var oldSb = _sb.ToString();
            _sb.Clear();

            _sb.Append(" ORDER BY ");

            if (select.OrderBy.IsNotNullAndNotEmpty())
            {
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
            else
            {
                _sb.Append("(SELECT 1)");
            }

            var orderByString = _sb.ToString();

            _sb.Clear();
            _sb.Append(oldSb);
            var tableNameAlias = "";
            //加入row_number开窗函数

            _sb.AppendFormat(",ROW_NUMBER() OVER({0}) AS [ROW] FROM ", orderByString);
            oldSb = _sb.ToString();
            _sb.Clear();

            if (select.From != null)
            {
                //_sb.Append(" FROM ");
                //_sb.Append("(");
                if (select.From is TableExpression table)
                {
                    var tableName = GetSchemaTableName(table.Schema, table.Name);
                    _sb.Append(tableName);

                    tableNameAlias = BoxTableName(select.Alias);
                    _sb.AppendFormat(" {0}", tableNameAlias);

                }
                else if (select.From is SelectExpression subSelectExpression)
                {
                    subSelectExpression.IsIgnoreOrderBy = true;
                    //this.RenameSelectExpressionInternalAlias(subSelectExpression);
                    tableNameAlias = BoxTableName(select.Alias);
                    _sb.Append("(");
                    this.VisitSelect(subSelectExpression);
                    _sb.AppendFormat(") {0}", tableNameAlias);

                }

            }
            else
            {
                throw new ArgumentException("loss from");
            }

            var hasWhere = false;
            if (select.Where != null)
            {
                hasWhere = true;
                _sb.Append(" WHERE ");
                this.VisitWhere(select.Where);
            }

            var tempSb = _sb.ToString();
            _sb.Clear();
            _sb.Append(oldSb);
            _sb.Append(tempSb);

            countSqlSb.Append($"select count(1) from {tempSb}");

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

            _sb.AppendFormat(") {0} WHERE {0}.[ROW]>", BoxColumnName(externalAlias));
            var hasSkip = select.Skip.HasValue;
            if (hasSkip)
            {
                _sb.Append(BoxParameter(select.Skip.Value, typeof(int)));
            }
            else
            {
                _sb.Append(BoxParameter(0, typeof(int)));
            }

            _sb.AppendFormat(" AND {0}.[ROW]<=", BoxColumnName(externalAlias));
            var theLast = select.Skip.GetValueOrDefault(0) + select.Take.GetValueOrDefault(0);
            _sb.Append(BoxParameter(theLast, typeof(int)));
        }

        /// <summary>
        /// 有跳过的分页
        /// </summary>
        /// <param name="select"></param>
        private void HasMultiQuerySkipPaging(SelectExpression select)
        {
            var externalAlias = this.GetNewAlias();

            _sb.Append("SELECT * FROM ( SELECT");
            
            int index = 0;

            index = 0;
            if (select.From is TableExpression fromTable)
            {
                index = 0;
                foreach (var column in select.Columns)
                {
                    if (index++ > 0) _sb.Append(", ");
                    this.VisitColumn(column);
                }
            }
            else
            {
                throw new NotSupportedException(nameof(select.From));
            }

            //提取orderBy
            var oldSb = _sb.ToString();
            _sb.Clear();

            _sb.Append(" ORDER BY ");

            if (select.OrderBy.IsNotNullAndNotEmpty())
            {
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
            else
            {
                _sb.Append("(SELECT 1)");
            }

            var orderByString = _sb.ToString();

            _sb.Clear();
            _sb.Append(oldSb);
            var tableNameAlias = "";
            //加入row_number开窗函数

            _sb.AppendFormat(",ROW_NUMBER() OVER({0}) AS [ROW] FROM ", orderByString);
            oldSb = _sb.ToString();
            _sb.Clear();

            if (select.From != null)
            {
                //_sb.Append(" FROM ");
                //_sb.Append("(");
                if (select.From is TableExpression table)
                {
                    var tableName = GetSchemaTableName(table.Schema, table.Name);
                    _sb.Append(tableName);

                    tableNameAlias = BoxTableName(select.Alias);
                    _sb.AppendFormat(" {0}", tableNameAlias);

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

            var tempSb = _sb.ToString();
            _sb.Clear();
            _sb.Append(oldSb);
            _sb.Append(tempSb);

            countSqlSb.Append($"select count(1) from {tempSb}");

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

            _sb.AppendFormat(") {0} WHERE {0}.[ROW]>", BoxColumnName(externalAlias));
            var hasSkip = select.Skip.HasValue;
            if (hasSkip)
            {
                _sb.Append(BoxParameter(select.Skip.Value, typeof(int)));
            }
            else
            {
                _sb.Append(BoxParameter(0, typeof(int)));
            }

            _sb.AppendFormat(" AND {0}.[ROW]<=", BoxColumnName(externalAlias));
            var theLast = select.Skip.GetValueOrDefault(0) + select.Take.GetValueOrDefault(0);
            _sb.Append(BoxParameter(theLast, typeof(int)));
        }

        /// <summary>
        /// 没有跳过分页
        /// </summary>
        /// <param name="select"></param>
        private void OnlyTakePaging(SelectExpression select)
        {
            _sb.Append("SELECT ");

            if (!select.ColumnsPrefix.IsNullOrWhiteSpace())
            {
                _sb.AppendFormat("{0} ", select.ColumnsPrefix);
            }

            _sb.AppendFormat("TOP({0}) ", select.Take.GetValueOrDefault(0));

            int index = 0;
            foreach (var column in select.Columns)
            {
                if (index++ > 0) _sb.Append(", ");
                this.VisitColumn(column);
            }

            if (select.From != null)
            {
                _sb.Append(" FROM ");
                //_sb.Append("(");
                if (select.From is TableExpression table)
                {
                    var tableName = GetSchemaTableName(table.Schema, table.Name);
                    _sb.Append(tableName);

                    var tableNameAlias = BoxTableName(select.Alias);
                    _sb.AppendFormat(" {0}", tableNameAlias);

                }
                else if (select.From is SelectExpression subSelectExpression)
                {
                    subSelectExpression.IsIgnoreOrderBy = true;
                    _sb.Append("(");

                    this.VisitSelect(subSelectExpression);
                    _sb.Append(")");
                    var tableNameAlias = BoxTableName(select.Alias);
                    _sb.AppendFormat(" {0}", tableNameAlias);
                }

            }
            else
            {
                throw new ArgumentException("loss from");
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
            if (select.Skip.HasValue)
            {
                this.HasSkipPaging(select);
            }
            else
            {
                OnlyTakePaging(select);
            }
        }

        protected void BoxPagination(SelectExpression select)
        {
            var orderByStringBuilder = new StringBuilder();
            if (select.OrderBy.IsNotNullAndNotEmpty())
            {
                orderByStringBuilder.Append(" ORDER BY ");
                for (var i = 0; i < select.OrderBy.Count; i++)
                {
                    var orderBy = select.OrderBy[i];
                    this.VisitColumn(orderBy.ColumnExpression);
                    orderByStringBuilder.Append(orderBy.OrderByType == OrderByType.Desc ? " DESC" : "");
                    if (i < select.OrderBy.Count - 1)
                    {
                        orderByStringBuilder.Append(",");
                    }
                }
            }

            var tempStringBuilder = new StringBuilder();
            tempStringBuilder.AppendFormat(
                @"select * from (select row_number() over (order by (select 1)) page_rn, page_inner.* FROM (SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] As [p0]) page_inner
            ) page_outer where page_rn > 5 and page_rn <= 10");

            _sb.Append(" LIMIT ");
            var hasSkip = select.Skip.HasValue;
            if (hasSkip)
            {
                _sb.Append(BoxParameter(select.Skip.Value, typeof(int)));
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
                _sb.Append(BoxParameter(int.MaxValue,typeof(int)));
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