using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using SummerBoot.Core;

namespace SummerBoot.Repository.ExpressionParser.Parser.Dialect
{
    public class OracleQueryFormatter : QueryFormatter
    {
        public OracleQueryFormatter(DatabaseUnit databaseUnit) : base(":", "\"", "\"",databaseUnit)
        {

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
            var externalAlias = this.GetNewAlias();

            _sb.Append("SELECT * FROM ( SELECT");

            var index = 0;
            var isFromTable = false;
            if (select.From is TableExpression fromTable)
            {
                isFromTable = true;

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
                _sb.Append(" null ");
            }

            var orderByString = _sb.ToString();

            _sb.Clear();
            _sb.Append(oldSb);
            var tableNameAlias = "";
            //加入row_number开窗函数

            _sb.AppendFormat(",ROW_NUMBER() OVER({0}) AS pageNo FROM ", orderByString);
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
                    ParseJoinCondition(_sb, joinExpression.JoinCondition);
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

            _sb.AppendFormat(") {0} WHERE {0}.pageNo>", BoxColumnName(externalAlias));
            var hasSkip = select.Skip.HasValue;
            if (hasSkip)
            {
                _sb.Append(BoxParameter(select.Skip.Value, typeof(int?)));
            }
            else
            {
                _sb.Append(BoxParameter(0, typeof(int)));
            }

            _sb.AppendFormat(" AND {0}.pageNo<=", BoxColumnName(externalAlias));
            var theLast = select.Skip.GetValueOrDefault(0) + select.Take.GetValueOrDefault(0);
            _sb.Append(BoxParameter(theLast, typeof(int)));
        }


        protected override void HandlingPaging(SelectExpression @select)
        {
            if (select.Joins.Any())
            {
                HandlingMulTiQueryPaging(select);
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
                _sb.Append(" null ");
            }

            var orderByString = _sb.ToString();

            _sb.Clear();
            _sb.Append(oldSb);
            var tableNameAlias = "";
            //加入row_number开窗函数

            _sb.AppendFormat(",ROW_NUMBER() OVER({0}) AS pageNo FROM ", orderByString);
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

            _sb.AppendFormat(") {0} WHERE {0}.pageNo>", BoxColumnName(externalAlias));
            var hasSkip = select.Skip.HasValue;
            if (hasSkip)
            {
                _sb.Append(BoxParameter(select.Skip.Value, typeof(int?)));
            }
            else
            {
                _sb.Append(BoxParameter(0, typeof(int)));
            }

            _sb.AppendFormat(" AND {0}.pageNo<=", BoxColumnName(externalAlias));
            var theLast = select.Skip.GetValueOrDefault(0) + select.Take.GetValueOrDefault(0);
            _sb.Append(BoxParameter(theLast, typeof(int)));
        }

        public override DbQueryResult Insert<T>(T insertEntity)
        {
            var result = base.Insert(insertEntity);
            if (result.IdKeyPropertyInfo != null)
            {

                result.Sql += $" RETURNING {BoxColumnName(result.IdName)} INTO {parameterPrefix}{result.IdName}";
            }

            return result;
        }

        public override DbQueryResult FastBatchInsert<T>(List<T> insertEntitys)
        {
            Clear();
            var table = this.getTableExpression(typeof(T));
            var tableName = GetSchemaTableName(table.Schema, table.Name);

            var parameterNameList = new List<string>();
            var columnNameList = new List<string>();
            var propertyNames = new List<string>();
            var propertyTypes = new List<Type>();
            var j = 1;

            foreach (var column in table.Columns)
            {

                if (column.IsKey && column.IsDatabaseGeneratedIdentity)
                {
                    continue;
                }

                propertyNames.Add(column.MemberInfo.Name);
                var type = (column.MemberInfo as PropertyInfo)!.PropertyType;
                propertyTypes.Add(type);
                var columnName = BoxColumnName(column.ColumnName);
                columnNameList.Add(columnName);
                var parameterName = this.parameterPrefix + j;
                j++;
                parameterNameList.Add(parameterName);
            }

            var allCount = insertEntitys.Count;

            for (int i = 0; i < propertyNames.Count; i++)
            {
                var propertyName = propertyNames[i];
                var propertyType = propertyTypes[i];
                var dbType = propertyType.CSharpTypeToDbType(DatabaseType.Oracle);
                //if (dbType == DbType.Time|| dbType == DbType.DateTime)
                //{
                //    continue;
                //}
                var arrayType = propertyType.MakeArrayType(1);
                var array = (Array)Activator.CreateInstance(arrayType, new object[1] { allCount });
                var k = 0;
                foreach (var insertEntity in insertEntitys)
                {
                    var propertyValue = insertEntity.GetPropertyValue(propertyName);
                    array!.SetValue(propertyValue, k);
                    k++;
                }
                
                this.sqlParameters.Add(new SqlParameter() { Value = array, DbType =dbType });
            }

            _sb.Append($"insert into {tableName} ({string.Join(",", columnNameList)}) values ({string.Join(",", parameterNameList)})");

            var result = new DbQueryResult()
            {
                Sql = this._sb.ToString().Trim(),
                SqlParameters = this.sqlParameters
            };

            return result;
        }
    }
}