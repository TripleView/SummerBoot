using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using SqlParser.Net.Ast.Expression;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public class DbQueryProvider : IAsyncQueryProvider
    {
        public IDbExecuteAndQuery linkRepository;
        public DatabaseUnit DatabaseUnit;
        private Func<Expression, WrapperExpression> getDbQueryResultByExpression;
        public DbQueryProvider(DatabaseUnit databaseUnit, IDbExecuteAndQuery linkRepository, Func<Expression, WrapperExpression> getDbQueryResultByExpression)
        {
            this.DatabaseUnit = databaseUnit;
            this.linkRepository = linkRepository;
            this.getDbQueryResultByExpression = getDbQueryResultByExpression;
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

        private WrapperExpression GetWrapperExpression(Expression expression)
        {
            var wrapperExpression = getDbQueryResultByExpression(expression);
            return wrapperExpression;
        } 

        public TResult Execute<TResult>(Expression expression)
        {
            var wrapperExpression = GetWrapperExpression(expression);
            var sql = wrapperExpression.SqlExpression.ToSql();
            var parameters = wrapperExpression.Parameters;
            return linkRepository.QueryFirstOrDefault<TResult>(sql, parameters);
        }

        public async Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var wrapperExpression = GetWrapperExpression(expression);
            var sql = wrapperExpression.SqlExpression.ToSql();
            var parameters = wrapperExpression.Parameters;
            return await linkRepository.QueryFirstOrDefaultAsync<TResult>(sql, parameters);
        }
    }
}