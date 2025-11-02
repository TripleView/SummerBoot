using SqlParser.Net.Ast.Expression;
using SummerBoot.Core;
using SummerBoot.Repository.Core;
using SummerBoot.Repository.ExpressionParser.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DbType = SqlParser.Net.DbType;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public class QueryFormatter
    {

        public QueryFormatter(string parameterPrefix, string leftQuote, string rightQuote, DatabaseUnit databaseUnit)
        {
            this.parameterPrefix = parameterPrefix;
            this.leftQuote = leftQuote;
            this.rightQuote = rightQuote;
            this.databaseUnit = databaseUnit;
        }

        protected DatabaseUnit databaseUnit;

        protected readonly StringBuilder _sb = new StringBuilder();
        protected readonly StringBuilder countSqlSb = new StringBuilder();

        protected DynamicParameters dynamicParameters = new DynamicParameters();
        protected string parameterPrefix;
        protected string leftQuote;
        protected string rightQuote;

        protected virtual string GetLastInsertIdSql()
        {
            return "";
        }

        protected string BoxColumnName(string columnName)
        {
            if (columnName == "*")
            {
                return columnName;
            }

            if (databaseUnit.ColumnNameMapping != null)
            {
                columnName = databaseUnit.ColumnNameMapping(columnName);
            }

            return CombineQuoteAndName(columnName);
        }

        protected string BoxTableName(string tableName)
        {
            if (databaseUnit.TableNameMapping != null)
            {
                tableName = databaseUnit.TableNameMapping(tableName);
            }
            return CombineQuoteAndName(tableName);
        }

        private string CombineQuoteAndName(string name)
        {
            return leftQuote + name + rightQuote;
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

        protected TableInfo GetTableInfo(Type type)
        {
            var key = "getTableInfo" + type.FullName;
            if (SbUtil.CacheDictionary.TryGetValue(key, out var cacheValue))
            {
                return (TableInfo)cacheValue;
            }

            var result = new TableInfo(type);
            SbUtil.CacheDictionary.TryAdd(key, result);
            return result;
        }


        protected string GetSchemaTableName(string schema, string tableName)
        {
            tableName = BoxTableName(tableName);
            tableName = schema.HasText() ? schema + "." + tableName : tableName;
            return tableName;
        }

        protected virtual DbType GetDbType()
        {
            switch (databaseUnit.DatabaseType)
            {
                case DatabaseType.Mysql:
                    return DbType.MySql;
                case DatabaseType.SqlServer:
                    return DbType.SqlServer;
                case DatabaseType.Sqlite:
                    return DbType.Sqlite;
                case DatabaseType.Pgsql:
                    return DbType.Pgsql;
                case DatabaseType.Oracle:
                    return DbType.Oracle;
                default:
                    throw new NotImplementedException();
            }
        }

        private T AppendQualifier<T>(T t) where T : IQualifierExpression
        {
            t.LeftQualifiers = leftQuote;
            t.RightQualifiers = rightQuote;
            return t;
        }

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="insertEntitys"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public virtual DbQueryResult FastBatchInsert<T>(List<T> insertEntitys)
        {
            throw new NotSupportedException("not support this database");
        }

        public DbQueryResult Delete<T>(T deleteEntity)
        {
            return null;
            //var table = this.GetTableExpression(typeof(T));
            //var tableName = GetSchemaTableName(table.Schema, table.Name);

            //var middleList = new List<string>();
            //foreach (var column in table.Columns)
            //{
            //    var columnName = BoxColumnName(column.ColumnName);
            //    var parameterName = this.parameterPrefix + column.MemberInfo.Name;
            //    if (column.MemberInfo is PropertyInfo propertyInfo)
            //    {
            //        if (propertyInfo.GetValue(deleteEntity) is null)
            //        {
            //            middleList.Add(columnName + " is null ");
            //            continue;
            //        }
            //    }
            //    middleList.Add(columnName + "=" + parameterName);
            //}

            //_sb.Append($"delete from {tableName} where {string.Join(" and ", middleList)}");

            //var result = new DbQueryResult()
            //{
            //    Sql = this._sb.ToString().Trim(),
            //    DynamicParameters = this.dynamicParameters,
            //};
            //return result;
        }

        public DbQueryResult DeleteByExpression<T>(Expression exp)
        {
            return null;
            //Clear();
            //var table = this.getTableExpression(typeof(T));
            //var tableName = GetSchemaTableName(table.Schema, table.Name);

            //var middleResult = this.Visit(exp);
            //this.FormatWhere(middleResult);
            //var whereSql = _sb.ToString();
            //var result = new DbQueryResult()
            //{
            //    SqlParameters = this.sqlParameters
            //};


            ////判断是否软删除
            ////软删除
            //if (RepositoryOption.Instance != null && RepositoryOption.Instance.IsUseSoftDelete && typeof(BaseEntity).IsAssignableFrom(typeof(T)))
            //{
            //    var column = table.Columns.FirstOrDefault(it => it.MemberInfo.Name == "Active");
            //    if (column != null)
            //    {
            //        var updateSql = $"update {tableName} set {BoxColumnName(column.ColumnName)}=0 where 1=1";
            //        if (!string.IsNullOrWhiteSpace(whereSql))
            //        {
            //            updateSql += $" and {whereSql}";
            //        }
            //        result.Sql = updateSql;
            //    }
            //}
            ////正常删除
            //else
            //{
            //    var deleteSql = $"delete from {tableName} where 1=1";
            //    if (!string.IsNullOrWhiteSpace(whereSql))
            //    {
            //        deleteSql += $" and {whereSql}";
            //    }

            //    result.Sql = deleteSql;

            //}

            //return result;
        }

        public DbQueryResult GetAll<T>()
        {
            return null;
        }

        public DbQueryResult Get<T>(object id)
        {
            return null;
        }

    }
}