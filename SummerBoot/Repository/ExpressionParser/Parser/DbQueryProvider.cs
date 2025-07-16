using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using SqlParser.Net;
using SummerBoot.Repository.ExpressionParser.Base;
using SummerBoot.Repository.ExpressionParser.Parser.Dialect;
using SummerBoot.Repository.ExpressionParser.Parser.MultiQuery;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public class DbQueryProvider : IAsyncQueryProvider
    {
        public QueryFormatter queryFormatter;
        private DbType dbType;
        public IDbExecuteAndQuery linkRepository;
        protected DatabaseUnit databaseUnit;

        private NewDbExpressionVisitor newDbExpressionVisitor;
        public DbQueryProvider(DatabaseUnit databaseUnit, IDbExecuteAndQuery linkRepository)
        {
            var databaseType = databaseUnit.DatabaseType;
            this.databaseUnit = databaseUnit;
            this.linkRepository = linkRepository;
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    dbType = DbType.SqlServer;
                    this.queryFormatter = new SqlServerQueryFormatter(databaseUnit);
                    break;
                case DatabaseType.Mysql:
                    dbType = DbType.MySql;
                    this.queryFormatter = new MysqlQueryFormatter(databaseUnit);
                    break;
                case DatabaseType.Oracle:
                    dbType = DbType.Oracle;
                    this.queryFormatter = new OracleQueryFormatter(databaseUnit);
                    break;
                case DatabaseType.Sqlite:
                    dbType = DbType.Sqlite;
                    this.queryFormatter = new SqliteQueryFormatter(databaseUnit);
                    break;
                case DatabaseType.Pgsql:
                    dbType = DbType.Pgsql;
                    this.queryFormatter = new PgsqlQueryFormatter(databaseUnit);
                    break;
            }

        }
        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }


        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new Repository<TElement>(expression, this);
        }

        public ExpressionTreeParsingResult GetDbQueryResultByExpression(Expression expression)
        {
            newDbExpressionVisitor = new NewDbExpressionVisitor(databaseUnit);
            newDbExpressionVisitor.Visit(expression);
            var parsingResult = newDbExpressionVisitor.GetParsingResult();
            return parsingResult;
        }

        public DbQueryResult GetJoinQueryResultByExpression<T>(IRepository<T> repository, IPageable pageable = null)
        {
            var expression = repository.Expression;
            //这一步将expression转化成我们自己的expression

            //join
            var dbExpressionVisitor = new DbExpressionVisitor();
            var queryBody = dbExpressionVisitor.Visit(expression);
            var joins = new List<JoinExpression>();
            foreach (var joinItem in repository.MultiQueryContext.JoinItems)
            {
                if (joinItem.Condition is LambdaExpression lambdaExpression)
                {

                }
                else
                {
                    throw new NotSupportedException(joinItem.Condition.ToString());
                }

                var joinAdapterExpression = new MultiQueryWhereAdapterExpression(lambdaExpression.Body);
                var visitor = new DbExpressionVisitor();
                var joinResult = visitor.Visit(joinAdapterExpression);
                var joinTable = new TableExpression(joinItem.JoinTable);
                if (joinResult is WhereExpression joinCondition)
                {
                    var joinExpression = new JoinExpression(joinItem.JoinType, joinTable, joinItem.JoinTableAlias);
                    joinExpression.JoinCondition = joinCondition;
                    joins.Add(joinExpression);
                }
            }

            //where
            WhereExpression whereExpression = null;
            if (repository.MultiQueryContext.MultiQueryWhereItem != null)
            {
                var visitor = new DbExpressionVisitor();
                if (repository.MultiQueryContext.MultiQueryWhereItem is LambdaExpression lambda)
                {
                    var multiQueryWhereAdapterExpression = new MultiQueryWhereAdapterExpression(lambda.Body);
                    var multiQueryWhereAdapterExpressionResult = visitor.Visit(multiQueryWhereAdapterExpression);

                    if (multiQueryWhereAdapterExpressionResult is WhereExpression tempWhereExpression)
                    {
                        whereExpression = tempWhereExpression;
                    }
                }
                else
                {
                    throw new NotSupportedException((repository.MultiQueryContext.MultiQuerySelectItem as Expression).Type.Name);
                }
            }

            var selectColumns = new List<ColumnExpression>();
            if (repository.MultiQueryContext.MultiQuerySelectItem != null)
            {
                var visitor = new DbExpressionVisitor();
                if (repository.MultiQueryContext.MultiQuerySelectItem is LambdaExpression lambda)
                {
                    var multiSelectExpression = new MultiSelectExpression(lambda.Body);
                    var multiSelectExpressionResult = visitor.Visit(multiSelectExpression);
                    if (multiSelectExpressionResult is ColumnsExpression columnsExpression)
                    {
                        selectColumns.AddRange(columnsExpression.ColumnExpressions);
                    }
                    else if (multiSelectExpressionResult is ColumnExpression columnExpression)
                    {
                        selectColumns.Add(columnExpression);
                    }
                }
                else
                {
                    throw new NotSupportedException((repository.MultiQueryContext.MultiQuerySelectItem as Expression).Type.Name);
                }
            }

            if (repository.MultiQueryContext.MultiQuerySelectAutoFillItem != null)
            {
                var visitor = new DbExpressionVisitor();
                if (repository.MultiQueryContext.MultiQuerySelectAutoFillItem is LambdaExpression lambda)
                {
                    var multiSelectAutoFillExpression = new MultiSelectAutoFillExpression(lambda.Body);
                    var multiSelectAutoFillExpressionResult = visitor.Visit(multiSelectAutoFillExpression);
                    if (multiSelectAutoFillExpressionResult is ColumnsExpression columnsExpression)
                    {
                        selectColumns.AddRange(columnsExpression.ColumnExpressions);
                    }
                }
                else
                {
                    throw new NotSupportedException((repository.MultiQueryContext.MultiQuerySelectItem as Expression).Type.Name);
                }
            }

            //order by
            var orderByExpressions = new List<OrderByExpression>();
            if (repository.MultiQueryContext.MultiQueryOrderByItems.Any())
            {
                foreach (var multiQueryOrderByItem in repository.MultiQueryContext.MultiQueryOrderByItems)
                {
                    var visitor = new DbExpressionVisitor();

                    if (multiQueryOrderByItem.OrderByExpression is LambdaExpression lambda)
                    {
                        var multiQueryOrderByAdapterExpression = new MultiQueryOrderByAdapterExpression(lambda.Body);
                        var multiQueryOrderByAdapterExpressionResult = visitor.Visit(multiQueryOrderByAdapterExpression);
                        if (multiQueryOrderByAdapterExpressionResult is ColumnExpression columnsExpression)
                        {
                            var orderByExpression = new OrderByExpression(orderByType: multiQueryOrderByItem.OrderByType, columnsExpression);
                            orderByExpressions.Add(orderByExpression);
                        }
                    }
                    else
                    {
                        throw new NotSupportedException((multiQueryOrderByItem.OrderByExpression as Expression).Type.Name);
                    }

                }




            }

            if (queryBody is TableExpression tableExpression)
            {
                var result = new SelectExpression(null, "T1", selectColumns, tableExpression,
                    joins: joins, where: whereExpression, orderBy: orderByExpressions);
                if (pageable != null)
                {
                    result.Skip = (pageable.PageNumber - 1) * pageable.PageSize;
                    result.Take = pageable.PageSize;
                }
                //将我们自己的expression转换成sql
                queryFormatter.Format(result);
                var param = queryFormatter.GetDbQueryDetail();
                return param;
            }

            throw new NotSupportedException();
        }

        public DbQueryResult GetDbPageQueryResultByExpression(Expression expression)
        {
            //这一步将expression转化成我们自己的expression
            var dbExpressionVisitor = new DbExpressionVisitor();
            var middleResult = dbExpressionVisitor.Visit(expression);
            if (middleResult is SelectExpression selectExpression)
            {
                if (!selectExpression.HasPagination)
                {
                    selectExpression.Skip = 0;
                    selectExpression.Take = int.MaxValue;
                }
            }
            //将我们自己的expression转换成sql
            queryFormatter.Format(middleResult);
            var param = queryFormatter.GetDbQueryDetail();
            return param;
        }

        public ExpressionTreeParsingResult GetParsingResult()
        {
            return newDbExpressionVisitor.GetParsingResult();
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var parsingResult = GetDbQueryResultByExpression(expression);
            return linkRepository.QueryFirstOrDefault<TResult>(parsingResult.Sql, parsingResult.Parameters);
        }

        public async Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var parsingResult = GetDbQueryResultByExpression(expression);
            return await linkRepository.QueryFirstOrDefaultAsync<TResult>(parsingResult.Sql, parsingResult.Parameters);
        }
    }
}