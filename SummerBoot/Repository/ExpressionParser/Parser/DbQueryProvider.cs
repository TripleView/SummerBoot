using System;
using System.Linq;
using System.Linq.Expressions;
using SummerBoot.Repository.ExpressionParser.Base;
using SummerBoot.Repository.ExpressionParser.Parser.Dialect;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public class DbQueryProvider : IQueryProvider
    {
        public QueryFormatter queryFormatter;

        public IDbExecuteAndQuery linkRepository;
        public DbQueryProvider(DatabaseType databaseType, IDbExecuteAndQuery linkRepository)
        {
            this.linkRepository = linkRepository;
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    this.queryFormatter = new SqlServerQueryFormatter();
                    break;
                case DatabaseType.Mysql:
                    this.queryFormatter = new MysqlQueryFormatter();
                    break;
                case DatabaseType.Oracle:
                    this.queryFormatter = new OracleQueryFormatter();
                    break;
                case DatabaseType.Sqlite:
                    this.queryFormatter = new SqliteQueryFormatter();
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
    }
}