using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ExpressionParser.Util;

namespace ExpressionParser.Parser
{
    public class QueryFormatter : DbExpressionVisitor, IDbExecute
    {

        public QueryFormatter(string parameterPrefix, string leftQuote, string rightQuote)
        {
            this.parameterPrefix = parameterPrefix;
            this.leftQuote = leftQuote;
            this.rightQuote = rightQuote;
        }

        protected readonly StringBuilder _sb = new StringBuilder();

        private readonly List<SqlParameter> sqlParameters = new List<SqlParameter>();

        private int parameterIndex = 0;
        private int selectIndex = 0;
        
        private string parameterPrefix;
        private string leftQuote;
        private string rightQuote;

        public PageQueryResult GetByPage(string sql)
        {
            var result = new PageQueryResult();
            return result;
        }
        protected virtual string GetLastInsertIdSql()
        {
            return "";
        }

        public override Expression VisitTable(TableExpression table)
        {
            _sb.Append("SELECT ");

            int index = 0;
            foreach (var column in table.Columns)
            {
                if (index++ > 0) _sb.Append(", ");
                this.VisitColumn(column);
            }
            _sb.AppendFormat(" FROM {0}", BoxTableNameOrColumnName(table.Name));
            //if (!table.Alias.IsNullOrWhiteSpace())
            //    _sb.AppendFormat(" As {0} ", BoxTableNameOrColumnName(table.Alias));

            return table;
        }
        /// <summary>
        /// 包装表名或者列名
        /// </summary>
        /// <param name="tableNameOrColumnName"></param>
        /// <returns></returns>

        protected string BoxTableNameOrColumnName(string tableNameOrColumnName)
        {
            if (tableNameOrColumnName == "*")
            {
                return tableNameOrColumnName;
            }

            return leftQuote + tableNameOrColumnName + rightQuote;
        }

        /// <summary>
        /// 重命名select里的所有列的别名
        /// </summary>
        /// <param name="selectExpression"></param>
        protected void RenameSelectExpressionInternalAlias(SelectExpression selectExpression)
        {
            var newAlias = this.GetNewAlias();
            selectExpression.Columns.ForEach(it=>it.TableAlias=newAlias);
            selectExpression.Alias = newAlias;
            selectExpression.GroupBy.Select(it=>it.ColumnExpression).ToList().ForEach(it=>it.TableAlias=newAlias);
            selectExpression.OrderBy.Select(it => it.ColumnExpression).ToList().ForEach(it => it.TableAlias = newAlias);
            RenameWhereColumnsAlias(selectExpression.Where, newAlias);
        }

        private void RenameWhereColumnsAlias(WhereExpression whereExpression,string alias)
        {
            if (whereExpression == null)
            {
                return;
            }

            if (whereExpression.Left != null && whereExpression.Right != null)
            {
                this.RenameWhereColumnsAlias(whereExpression.Left,alias);
             
                this.RenameWhereColumnsAlias(whereExpression.Right, alias);
            }

            else if (whereExpression is WhereConditionExpression whereConditionExpression)
            {
                whereConditionExpression.ColumnExpression.TableAlias = alias;
            }
            else if (whereExpression is FunctionWhereConditionExpression functionWhereConditionExpression)
            {
                RenameWhereColumnsAlias(functionWhereConditionExpression.WhereExpression, alias);
            }
        }
        /// <summary>
        /// 获取函数别名，比如sqlserver就是LEN，mysql就是LENGTH
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        protected virtual string GetFunctionAlias(string functionName)
        {
            return functionName;
        }

        public void Format(Expression expression)
        {
            _sb.Clear();
            if (expression is SelectExpression selectExpression)
            {
                var result = this.VisitSelect(selectExpression);
            }
            else
            {
                throw new NotSupportedException(nameof(expression));
            }


        }

        public DbQueryResult GetDbQueryDetail()
        {
            return new DbQueryResult()
            {
                Sql = this._sb.ToString().Trim(),
                SqlParameters = this.sqlParameters
            };
        }

        public override Expression VisitQuery(QueryExpression queryExpression)
        {
            switch (queryExpression)
            {
                case SelectExpression selectExpression: this.VisitSelect(selectExpression); break;
            }

            return queryExpression;
        }

        /// <summary>
        /// 包装参数
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="returnRealValue">是否返回实际值</param>
        /// <returns></returns>
        protected string BoxParameter(object obj, bool returnRealValue = false)
        {
            var value = obj.ToString();
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }
            if (char.IsNumber(value, 0))
            {
                value = value;
            }
            else
            {
                value = $"'{value}'";
            }

            if (returnRealValue)
            {
                return value;
            }

            var finalValue = obj;
            if (obj is bool objBool)
            {
                finalValue = objBool ? 1 : 0;
            }

            var parameterName = this.parameterPrefix + "y" + parameterIndex;
            var sqlParameter = new SqlParameter()
            {
                ParameterName = parameterName,
                ParameterType = obj.GetType(),
                Value = finalValue
            };
            this.sqlParameters.Add(sqlParameter);
            parameterIndex++;

            return parameterName;
        }

        public override Expression VisitColumn(ColumnExpression columnExpression)
        {
            var tempStringBuilder = new StringBuilder();
            //如果有固定值,比如'福建' as address
            var value = columnExpression.Value;
            if (value != null)
            {
                tempStringBuilder.AppendFormat("{0} As {1}", BoxParameter(value, true), BoxTableNameOrColumnName(columnExpression.ColumnName));
            }
            else
            {
                if (!columnExpression.TableAlias.IsNullOrWhiteSpace()&&columnExpression.ColumnName!= "*")
                {
                    tempStringBuilder.AppendFormat("{0}.{1}", BoxTableNameOrColumnName(columnExpression.TableAlias), BoxTableNameOrColumnName(columnExpression.ColumnName));
                }
                else
                {
                    tempStringBuilder.AppendFormat("{0}", BoxTableNameOrColumnName(columnExpression.ColumnName));
                }
            }

            if (!columnExpression.FunctionName.IsNullOrWhiteSpace())
            {
                tempStringBuilder.Insert(0, this.GetFunctionAlias(columnExpression.FunctionName) + "(");
                tempStringBuilder.Insert(tempStringBuilder.Length, ")");
            }

            //添加列别名
            if (!columnExpression.ColumnAlias.IsNullOrWhiteSpace())
            {
                tempStringBuilder.AppendFormat(" As {0}", BoxTableNameOrColumnName(columnExpression.ColumnAlias));
            }

            _sb.Append(tempStringBuilder);

            return columnExpression;
        }

        /// <summary>
        /// 处理分页
        /// </summary>
        protected virtual void HandlingPaging(SelectExpression select)
        {

        }

        /// <summary>
        /// 处理正常逻辑
        /// </summary>
        protected void HandlingNormal(SelectExpression select)
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
                    _sb.Append(BoxTableNameOrColumnName(table.Name));
                    _sb.AppendFormat(" As {0}", BoxTableNameOrColumnName(select.Alias));
                }
                else if (select.From is SelectExpression subSelectExpression)
                {
                    _sb.Append("(");
                    this.VisitSelect(subSelectExpression);
                    _sb.AppendFormat(") As {0}", BoxTableNameOrColumnName(select.Alias));
                }

            }
            else
            {
                throw new ArgumentException("loss from");
            }

            if (select.Where != null)
            {
                _sb.Append(" WHERE ");
                this.VisitWhere(select.Where);
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

            if (select.OrderBy.IsNotNullAndNotEmpty()&&!select.IsIgnoreOrderBy)
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

        public override Expression VisitSelect(SelectExpression select)
        {
            RenameSelectExpressionInternalAlias(select);

            if (select.HasPagination)
            {
                HandlingPaging(select);
            }
            else
            {
                HandlingNormal(select);
            }

            return select;
        }

        public override Expression VisitWhere(WhereExpression whereExpression)
        {

            int index = 0;
            _sb.Append(" (");
            if (whereExpression.Left != null && whereExpression.Right != null)
            {
                this.VisitWhere(whereExpression.Left);
                _sb.Append(" ");
                _sb.Append(whereExpression.Operator);
                _sb.Append(" ");
                this.VisitWhere(whereExpression.Right);
                _sb.Append(" ");
            }

            else if (whereExpression is WhereConditionExpression whereConditionExpression)
            {
                this.VisitColumn(whereConditionExpression.ColumnExpression);
                _sb.Append(" ");
                _sb.Append(whereConditionExpression.Operator);
                _sb.Append(" ");
                _sb.Append(BoxParameter(whereConditionExpression.Value));
            }
            else if (whereExpression is FunctionWhereConditionExpression functionWhereConditionExpression)
            {
                _sb.Append(GetFunctionAlias(functionWhereConditionExpression.Operator) + " ");
                _sb.Append(" (");
                this.VisitWhere(functionWhereConditionExpression.WhereExpression);
                _sb.Append(" )");
            }

            _sb.Append(" )");


            return whereExpression;
        }

        private TableExpression getTableExpression(Type type)
        {
            return new TableExpression(type);
        }

        private void Clear()
        {
            this._sb.Clear();
            this.sqlParameters.Clear();
        }

        public DbQueryResult Insert<T>(T insertEntity)
        {
            Clear();
            var table = this.getTableExpression(typeof(T));
            var tableName = BoxTableNameOrColumnName(table.Name);

            var parameterNameList = new List<string>();
            var columnNameList = new List<string>();

            foreach (var column in table.Columns)
            {
                var columnName = BoxTableNameOrColumnName(column.ColumnName);
                columnNameList.Add(columnName);
                var parameterName = this.parameterPrefix + column.MemberInfo.Name;
                parameterNameList.Add(parameterName);
            }

            _sb.Append($"insert into {tableName} ({string.Join(",",columnNameList)}) values ({string.Join(",",parameterNameList)})");

            var result = new DbQueryResult()
            {
                Sql = this._sb.ToString().Trim(),
                SqlParameters = this.sqlParameters
            };

            var keyColumn = table.Columns.FirstOrDefault(it => it.IsKey && it.ColumnName.ToLower() == "id"&&it.MemberInfo is PropertyInfo);
            if (keyColumn != null)
            {
                result.LastInsertIdSql = GetLastInsertIdSql();
                result.IdKeyPropertyInfo = keyColumn.MemberInfo as PropertyInfo;
            }

            return result;
        }

        public DbQueryResult Update<T>(T updateEntity)
        {
            Clear();
            var table = this.getTableExpression(typeof(T));
            var tableName = BoxTableNameOrColumnName(table.Name);

            var columnNameList = new List<string>();
            var keyColumnNameList = new List<string>();

            var columns = table.Columns.Where(it => !it.IsKey).ToList();

            var keyColumnExpression = table.Columns.Where(it => it.IsKey).ToList();

            if (!keyColumnExpression.Any())
            {
                throw new Exception("请设置主键");
            }

            var middleList = new List<string>();
            foreach (var column in columns)
            {
                var columnName = BoxTableNameOrColumnName(column.ColumnName);
                columnNameList.Add(columnName);
                var parameterName = this.parameterPrefix + column.MemberInfo.Name;
                middleList.Add(columnName + "="+ parameterName);
            }

            var keyList = new List<string>();
            foreach (var column in keyColumnExpression)
            {
                var columnName = BoxTableNameOrColumnName(column.ColumnName);
                keyColumnNameList.Add(columnName);
                var parameterName = this.parameterPrefix + column.MemberInfo.Name;
                keyList.Add(columnName + "=" + parameterName);
            }

            _sb.Append($"update {tableName} set {string.Join(",",middleList)} where {string.Join(" and ", keyList)}");
            
            var result = new DbQueryResult()
            {
                Sql = this._sb.ToString().Trim(),
                SqlParameters = this.sqlParameters
            };
            return result;
        }

        public DbQueryResult Delete<T>(T deleteEntity)
        {
            Clear();
            var table = this.getTableExpression(typeof(T));
            var tableName = BoxTableNameOrColumnName(table.Name);


            var columnNameList = new List<string>();

            var middleList = new List<string>();
            foreach (var column in table.Columns)
            {
                var columnName = BoxTableNameOrColumnName(column.ColumnName);
                columnNameList.Add(columnName);
                var parameterName = this.parameterPrefix + column.MemberInfo.Name;
                middleList.Add(columnName + "=" + parameterName);
            }

            _sb.Append($"delete from {tableName} where {string.Join(" and ", middleList)}");

            var result = new DbQueryResult()
            {
                Sql = this._sb.ToString().Trim(),
                SqlParameters = this.sqlParameters
            };
            return result;
        }

        public DbQueryResult GetAll<T>()
        {
            Clear();
            var table = this.getTableExpression(typeof(T));
            var tableName = BoxTableNameOrColumnName(table.Name);
            var columnNameList = new List<string>();

            foreach (var column in table.Columns)
            {
                var columnName = BoxTableNameOrColumnName(column.ColumnName);
                columnNameList.Add(columnName);
            }

            _sb.Append($"select {string.Join(",", columnNameList)} from {tableName}");

            var result = new DbQueryResult()
            {
                Sql = this._sb.ToString().Trim(),
                SqlParameters = this.sqlParameters
            };
            return result;
        }

        public DbQueryResult Get<T>(dynamic id)
        {
            Clear();
            var table = this.getTableExpression(typeof(T));
            var tableName = BoxTableNameOrColumnName(table.Name);
            var columnNameList = new List<string>();

            foreach (var column in table.Columns)
            {
                var columnName = BoxTableNameOrColumnName(column.ColumnName);
                columnNameList.Add(columnName);
            }

            var keyColumn = table.Columns.FirstOrDefault(it => it.IsKey&&it.ColumnName.ToLower()=="id");
            if (keyColumn == null)
            {
                throw new Exception("not exist key column like id");
            }

            var parameterName = BoxParameter(id);

            _sb.Append($"select {string.Join(",", columnNameList)} from {tableName} where {BoxTableNameOrColumnName(keyColumn.ColumnName)}={parameterName}");

            var result = new DbQueryResult()
            {
                Sql = this._sb.ToString().Trim(),
                SqlParameters = this.sqlParameters
            };
            return result;
        }

    }
}