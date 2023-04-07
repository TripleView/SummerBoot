using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using SummerBoot.Repository.ExpressionParser.Base;
using SummerBoot.Repository.ExpressionParser.Parser.Dialect;

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
            return new Repository<TElement>(expression,this);
        }

        public DbQueryResult GetDbQueryResultByExpression(Expression expression)
        {
            //这一步将expression转化成我们自己的expression
            var dbExpressionVisitor = new DbExpressionVisitor();
            var middleResult = dbExpressionVisitor.Visit(expression);
            //将我们自己的expression转换成sql
            queryFormatter.Format(middleResult);
            var param = queryFormatter.GetDbQueryDetail();
            return param;
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