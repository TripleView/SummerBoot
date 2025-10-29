using SqlParser.Net.Ast.Expression;
using SummerBoot.Core;
using SummerBoot.Repository.Core;
using SummerBoot.Repository.ExpressionParser.Parser.MultiQuery;
using SummerBoot.Repository.ExpressionParser.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using YamlDotNet.Core.Tokens;
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

        protected TableExpression GetTableExpression(Type type)
        {
            var key = "getTableExpression" + type.FullName;
            if (SbUtil.CacheDictionary.TryGetValue(key, out var cacheValue))
            {
                return (TableExpression)cacheValue;
            }

            var result = new TableExpression(type);
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

        public virtual DbQueryResult Insert<T>(T insertEntity)
        {
            var key = "GetSqlInsertExpression" + typeof(T).FullName;
            var cacheResult = (DbQueryResult)SbUtil.CacheDictionary.GetOrAdd(key, x =>
             {
                 var table = this.GetTableExpression(typeof(T));
                 var dbType = GetDbType();
                 var tableExpression = new SqlTableExpression()
                 {
                     Name = new SqlIdentifierExpression()
                     {
                         Value = BoxTableName(table.Name)
                     }
                 };
                 if (table.Schema.HasText())
                 {
                     tableExpression.Schema = new SqlIdentifierExpression()
                     {
                         Value = BoxTableName(table.Schema)
                     };
                 }
                 var insertExpression = new SqlInsertExpression()
                 {
                     DbType = dbType,
                     Table = tableExpression
                 };
                 var valueeExpressions = new List<SqlExpression>();
                 foreach (var column in table.Columns)
                 {
                     var columnName = column.ColumnName;
                     if (column.IsKey && column.IsDatabaseGeneratedIdentity && !databaseUnit.IsDataMigrateMode)
                     {
                         continue;
                     }
                     insertExpression.Columns.Add(new SqlIdentifierExpression()
                     {
                         DbType = dbType,
                         LeftQualifiers = this.leftQuote,
                         Value = columnName,
                         RightQualifiers = this.rightQuote
                     });

                     valueeExpressions.Add(new SqlVariableExpression()
                     {
                         DbType = dbType,
                         Prefix = this.parameterPrefix,
                         Name = columnName
                     });
                 }
                 insertExpression.ValuesList.Add(valueeExpressions);

                 var result = new DbQueryResult()
                 {
                     ExecuteSqlExpression = insertExpression,
                 };

                 var keyColumn = table.Columns.FirstOrDefault(it => it.IsKey && it.IsDatabaseGeneratedIdentity && it.ColumnName.ToLower() == "id" && it.MemberInfo is PropertyInfo);
                 if (keyColumn != null)
                 {
                     if (dbType == DbType.SqlServer)
                     {
                         result.LastInsertIdSql = "select SCOPE_IDENTITY() id";
                     }
                     result.LastInsertIdSql = GetLastInsertIdSql();
                     result.IdKeyPropertyInfo = keyColumn.MemberInfo as PropertyInfo;
                     result.IdName = keyColumn.ColumnName;
                 }

                 return result;
             });

            cacheResult.DynamicParameters = new DynamicParameters(insertEntity);

            return cacheResult;
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

        public DbQueryResult Update<T>(T updateEntity)
        {
            var key = "GetSqlUpdateExpression" + typeof(T).FullName;
            var cacheResult = (DbQueryResult)SbUtil.CacheDictionary.GetOrAdd(key, x =>
            {
                var table = this.GetTableExpression(typeof(T));
                var dbType = GetDbType();

                var keyColumns = table.Columns.Where(it => it.IsKey).ToList();

                if (!keyColumns.Any())
                {
                    throw new Exception("Please set the primary key");
                }

                var tableExpression = new SqlTableExpression()
                {
                    Name = new SqlIdentifierExpression()
                    {
                        Value = BoxTableName(table.Name)
                    }
                };
                if (table.Schema.HasText())
                {
                    tableExpression.Schema = new SqlIdentifierExpression()
                    {
                        Value = BoxTableName(table.Schema)
                    };
                }
                var updateExpression = new SqlUpdateExpression()
                {
                    DbType = dbType,
                    Table = tableExpression
                };

                var columns = table.Columns.Where(it => !it.IsKey && !it.IsIgnoreWhenUpdate).ToList();


                foreach (var column in columns)
                {
                    var columnName = column.ColumnName;

                    updateExpression.Items.Add(new SqlBinaryExpression()
                    {
                        Left = new SqlIdentifierExpression()
                        {
                            DbType = dbType,
                            LeftQualifiers = this.leftQuote,
                            Value = columnName,
                            RightQualifiers = this.rightQuote
                        },
                        Operator = SqlBinaryOperator.EqualTo,
                        Right = new SqlVariableExpression()
                        {
                            DbType = dbType,
                            Prefix = this.parameterPrefix,
                            Name = columnName
                        }
                    });
                }

                var keyList = new List<string>();
                foreach (var column in keyColumns)
                {
                    if (column.MemberInfo is PropertyInfo propertyInfo)
                    {
                        var columnName = column.ColumnName;
                        if (propertyInfo.GetValue(updateEntity) is null)
                        {
                            var condition = new SqlBinaryExpression()
                            {
                                Left = new SqlIdentifierExpression()
                                {
                                    DbType = dbType,
                                    LeftQualifiers = this.leftQuote,
                                    Value = columnName,
                                    RightQualifiers = this.rightQuote
                                },
                                Operator = SqlBinaryOperator.Is,
                                Right = new SqlNullExpression()
                                {
                                    DbType = dbType
                                }
                            };
                            if (updateExpression.Where == null)
                            {
                                updateExpression.Where = condition;
                            }
                            else
                            {
                                updateExpression.Where =
                                    new SqlBinaryExpression()
                                    {
                                        Left = updateExpression.Where,
                                        Operator = SqlBinaryOperator.And,
                                        Right = condition
                                    };
                            }
                        }
                        else
                        {
                            var condition = new SqlBinaryExpression()
                            {
                                Left = new SqlIdentifierExpression()
                                {
                                    DbType = dbType,
                                    LeftQualifiers = this.leftQuote,
                                    Value = columnName,
                                    RightQualifiers = this.rightQuote
                                },
                                Operator = SqlBinaryOperator.EqualTo,
                                Right = new SqlVariableExpression()
                                {
                                    DbType = dbType,
                                    Prefix = this.parameterPrefix,
                                    Name = columnName
                                }
                            };
                            if (updateExpression.Where == null)
                            {
                                updateExpression.Where = condition;
                            }
                            else
                            {
                                updateExpression.Where =
                                    new SqlBinaryExpression()
                                    {
                                        Left = updateExpression.Where,
                                        Operator = SqlBinaryOperator.And,
                                        Right = condition
                                    };
                            }

                        }
                    }

                }

                var result = new DbQueryResult()
                {
                    ExecuteSqlExpression = updateExpression
                };

                return result;
            });

            cacheResult.DynamicParameters = new DynamicParameters(updateEntity);
            return cacheResult;
        }

        public DbQueryResult Delete<T>(T deleteEntity)
        {
            var table = this.GetTableExpression(typeof(T));
            var tableName = GetSchemaTableName(table.Schema, table.Name);

            var middleList = new List<string>();
            foreach (var column in table.Columns)
            {
                var columnName = BoxColumnName(column.ColumnName);
                var parameterName = this.parameterPrefix + column.MemberInfo.Name;
                if (column.MemberInfo is PropertyInfo propertyInfo)
                {
                    if (propertyInfo.GetValue(deleteEntity) is null)
                    {
                        middleList.Add(columnName + " is null ");
                        continue;
                    }
                }
                middleList.Add(columnName + "=" + parameterName);
            }

            _sb.Append($"delete from {tableName} where {string.Join(" and ", middleList)}");

            var result = new DbQueryResult()
            {
                Sql = this._sb.ToString().Trim(),
                DynamicParameters = this.dynamicParameters,
            };
            return result;
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

        public DbQueryResult ExecuteUpdate<T>(Expression expression, List<SelectItem<T>> selectItems)
        {
            return null;
            //Clear();
            //var table = this.getTableExpression(typeof(T));
            //var tableName = GetSchemaTableName(table.Schema, table.Name);

            //var middleResult = this.Visit(expression);
            //this.FormatWhere(middleResult);
            //var whereSql = _sb.ToString();
            //_sb.Clear();

            //var columnSetValueClauses = new List<string>();

            //foreach (var selectItem in selectItems)
            //{
            //    if (selectItem.Select.Body is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
            //    {
            //        var body = unaryExpression.Operand;
            //        var bodyResultExpression = this.Visit(body);
            //        if (bodyResultExpression is ColumnExpression columnExpression)
            //        {
            //            this.VisitColumn(columnExpression);
            //            _sb.Append("=");
            //            _sb.Append(BoxParameter(selectItem.Value, columnExpression.ValueType));
            //            var columnSetValueClause = _sb.ToString();
            //            columnSetValueClauses.Add(columnSetValueClause);
            //            _sb.Clear();
            //        }
            //    }
            //    else if (selectItem.Select.Body is MemberExpression memberExpression)
            //    {
            //        var bodyResultExpression = this.Visit(memberExpression);
            //        if (bodyResultExpression is ColumnExpression columnExpression)
            //        {
            //            this.VisitColumn(columnExpression);
            //            _sb.Append("=");
            //            _sb.Append(BoxParameter(selectItem.Value, columnExpression.ValueType));
            //            var columnSetValueClause = _sb.ToString();
            //            columnSetValueClauses.Add(columnSetValueClause);
            //            _sb.Clear();
            //        }
            //    }
            //    else
            //    {
            //        throw new NotSupportedException("setValue only support one property,like it=>it.name");
            //    }
            //}
            //var setValues = string.Join(",", columnSetValueClauses);

            //var deleteSql = $"update {tableName} set {setValues} where 1=1";
            //if (!string.IsNullOrWhiteSpace(whereSql))
            //{
            //    deleteSql += $" and {whereSql}";
            //}

            //var result = new DbQueryResult()
            //{
            //    Sql = deleteSql.Trim(),
            //    SqlParameters = this.sqlParameters
            //};
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