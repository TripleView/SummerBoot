using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using SummerBoot.Repository.ExpressionParser.Base;
using SummerBoot.Repository.ExpressionParser.Parser.Dialect;
using SummerBoot.Repository.ExpressionParser.Parser.MultiQuery;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public class DbQueryProvider : IAsyncQueryProvider
    {
        public QueryFormatter queryFormatter;

        public IDbExecuteAndQuery linkRepository;
        public DbQueryProvider(DatabaseUnit databaseUnit, IDbExecuteAndQuery linkRepository)
        {
            var databaseType = databaseUnit.DatabaseType;
            this.linkRepository = linkRepository;
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    this.queryFormatter = new SqlServerQueryFormatter(databaseUnit);
                    break;
                case DatabaseType.Mysql:
                    this.queryFormatter = new MysqlQueryFormatter(databaseUnit);
                    break;
                case DatabaseType.Oracle:
                    this.queryFormatter = new OracleQueryFormatter(databaseUnit);
                    break;
                case DatabaseType.Sqlite:
                    this.queryFormatter = new SqliteQueryFormatter(databaseUnit);
                    break;
                case DatabaseType.Pgsql:
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

        public DbQueryResult GetDbQueryResultByExpression(Expression expression)
        {
            //这一步将expression转化成我们自己的expression
            var dbExpressionVisitor = new DbExpressionVisitor();
            var middleResult = dbExpressionVisitor.Visit(expression);
            //var cc=dbExpressionVisitor.Visit(joinItems.)
            //将我们自己的expression转换成sql
            queryFormatter.Format(middleResult);
            var param = queryFormatter.GetDbQueryDetail();
            return param;
        }


        public DbQueryResult GetJoinQueryResultByExpression<T>(IRepository<T> repository)
        {
            var expression = repository.Expression;
            //这一步将expression转化成我们自己的expression
            var dbExpressionVisitor = new DbExpressionVisitor();
            var queryBody = dbExpressionVisitor.Visit(expression);
            var joins = new List<JoinExpression>();
            foreach (var joinItem in repository.JoinItems)
            {
                var joinAdapterExpression = new JoinAdapterExpression(joinItem.Condition as Expression);
                var visitor = new DbExpressionVisitor();
                var joinResult = visitor.Visit(joinAdapterExpression) ;
                var joinTable= new TableExpression(joinItem.JoinTable);
                if (joinResult is JoinConditionExpression joinCondition)
                {
                    var joinExpression = new JoinExpression(joinItem.JoinType, joinTable, joinItem.JoinTableAlias);
                    joinExpression.JoinCondition = joinCondition;
                    joins.Add(joinExpression);
                }
            }

            var selectColumns = new List<ColumnExpression>();
            if (repository.MultiQuerySelectItem != null)
            {
                var visitor = new DbExpressionVisitor();
                if (repository.MultiQuerySelectItem is LambdaExpression lambda)
                {
                    var multiSelectExpression = new MultiSelectExpression( lambda.Body);
                    var multiSelectExpressionResult = visitor.Visit(multiSelectExpression);
                    if (multiSelectExpressionResult is ColumnsExpression columnsExpression)
                    {
                        selectColumns.AddRange(columnsExpression.ColumnExpressions);
                    }
                }
                else
                {
                    throw new NotSupportedException((repository.MultiQuerySelectItem as Expression).Type.Name);
                }
               
            }
            
            if (queryBody is TableExpression tableExpression)
            {
                var result = new SelectExpression(null, "T1", selectColumns, tableExpression,
                    joins: joins);
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

        public DbQueryResult GetDbQueryDetail()
        {
            return queryFormatter.GetDbQueryDetail();
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            //这一步将expression转化成我们自己的expression
            var dbExpressionVisitor = new DbExpressionVisitor();
            var middleResult = dbExpressionVisitor.Visit(expression);
            //将我们自己的expression转换成sql
            queryFormatter.Format(middleResult);
            var param = queryFormatter.GetDbQueryDetail();

            return linkRepository.InternalQuery<TResult>(param);
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var dbExpressionVisitor = new DbExpressionVisitor();
            var middleResult = dbExpressionVisitor.Visit(expression);
            //将我们自己的expression转换成sql
            queryFormatter.Format(middleResult);
            var param = queryFormatter.GetDbQueryDetail();

            return linkRepository.InternalQueryAsync<TResult>(param);
        }
    }
}