using System;
using System.Collections;
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
            Type elementType = expression.Type.GetGenericArguments()[0];
            var queryableType = typeof(SummerbootQueryable<>).MakeGenericType(elementType);
            return (IQueryable)Activator.CreateInstance(queryableType, this, expression);
        }


        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) 
        {
            var result = new SummerbootQueryable<TElement>(expression, this);
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

        public IEnumerable<T> QueryList<T>(Expression expression)
        {
            var wrapperExpression = GetWrapperExpression(expression);
            var sql = wrapperExpression.SqlExpression.ToSql();
            var parameters = wrapperExpression.Parameters;
            return linkRepository.QueryList<T>(sql, parameters);
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


    public class SummerbootQueryable<T> : IQueryable<T>
    {
        private readonly Expression expression;
        private readonly DbQueryProvider provider;

        public SummerbootQueryable(DbQueryProvider provider)
        {
            provider = provider;
            expression = Expression.Constant(this);
        }

        public SummerbootQueryable(Expression expression,DbQueryProvider provider)
        {
            provider = provider;
            expression = expression;
        }

        public Type ElementType => typeof(T);
        public Expression Expression => expression;
        public IQueryProvider Provider => provider;

        public IEnumerator<T> GetEnumerator()
        {
            return provider.QueryList<T>(expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}