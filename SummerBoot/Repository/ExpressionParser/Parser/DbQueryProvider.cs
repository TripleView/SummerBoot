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
        public IDbExecuteAndQuery linkRepository;
        public DatabaseUnit DatabaseUnit;
        private Func<Expression, ExpressionTreeParsingResult> getDbQueryResultByExpression;
        protected bool IsDebug = false;
        public DbQueryProvider(DatabaseUnit databaseUnit, IDbExecuteAndQuery linkRepository, Func<Expression, ExpressionTreeParsingResult> getDbQueryResultByExpression, bool isDebug = false)
        {
            this.DatabaseUnit = databaseUnit;
            this.linkRepository = linkRepository;
            this.getDbQueryResultByExpression = getDbQueryResultByExpression;
            this.IsDebug = isDebug;
        }
        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }


        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var result = new Repository<TElement>(expression, this);
            return result;
        }

        public DbQueryResult GetJoinQueryResultByExpression<T>(IRepository<T> repository, IPageable pageable = null)
        {
            var expression = repository.Expression;
            //这一步将expression转化成我们自己的expression



            throw new NotSupportedException();
        }

        public DbQueryResult GetDbPageQueryResultByExpression(Expression expression)
        {
            //这一步将expression转化成我们自己的expression
            throw new NotSupportedException();
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var parsingResult = getDbQueryResultByExpression(expression);
            return linkRepository.QueryFirstOrDefault<TResult>(parsingResult.Sql, parsingResult.Parameters);
        }

        public async Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var parsingResult = getDbQueryResultByExpression(expression);
            return await linkRepository.QueryFirstOrDefaultAsync<TResult>(parsingResult.Sql, parsingResult.Parameters);
        }
    }
}