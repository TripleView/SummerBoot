using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public class DbQueryProvider : IAsyncQueryProvider
    {
        public IDbExecuteAndQuery linkRepository;
        public DatabaseUnit DatabaseUnit;
     
        public DbQueryProvider(DatabaseUnit databaseUnit, IDbExecuteAndQuery linkRepository)
        {
            this.DatabaseUnit = databaseUnit;
            this.linkRepository = linkRepository;
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
            var wrapperExpression = GetDbQueryResultByExpression(expression);
            var sql = wrapperExpression.SqlExpression.ToSql();
            var parameters = wrapperExpression.Parameters;
            return linkRepository.QueryFirstOrDefault<TResult>(sql, parameters);
        }

        public async Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var wrapperExpression = GetDbQueryResultByExpression(expression);
            var sql = wrapperExpression.SqlExpression.ToSql();
            var parameters = wrapperExpression.Parameters;
            return await linkRepository.QueryFirstOrDefaultAsync<TResult>(sql, parameters);
        }

        /// <summary>
        /// 执行删除或者更新
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<int> ExecuteDeleteOrUpdateAsync(Expression expression,
            CancellationToken cancellationToken = default)
        {
            var wrapperExpression = GetDbQueryResultByExpression(expression);
            var sql = wrapperExpression.SqlExpression.ToSql();
            var parameters = wrapperExpression.Parameters;
            return await linkRepository.ExecuteAsync(sql, parameters);
        }

        /// <summary>
        /// 执行删除或者更新
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public int ExecuteDeleteOrUpdate(Expression expression, CancellationToken cancellationToken = default)
        {
            var wrapperExpression = GetDbQueryResultByExpression(expression);
            var sql = wrapperExpression.SqlExpression.ToSql();
            var parameters = wrapperExpression.Parameters;
            return linkRepository.Execute(sql, parameters);
        }

        /// <summary>
        /// 将结果集转化为列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public async Task<List<T>> ToListAsync<T>(Expression expression)
        {
            var wrapperExpression = this.GetDbQueryResultByExpression(expression);
            var sql = wrapperExpression.SqlExpression.ToSql();
            var parameters = wrapperExpression.Parameters;
            var result = await linkRepository.QueryListAsync<T>(sql, parameters);
            return result;
        }

        public List<T> QueryList<T>(Expression expression)
        {
            var wrapperExpression = GetDbQueryResultByExpression(expression);
            var sql = wrapperExpression.SqlExpression.ToSql();
            var parameters = wrapperExpression.Parameters;
            return linkRepository.QueryList<T>(sql, parameters);
        }

        private WrapperExpression GetDbQueryResultByExpression(Expression expression)
        {
            var newDbExpressionVisitor = new NewDbExpressionVisitor(DatabaseUnit);
            var exp = newDbExpressionVisitor.Visit(expression);
            if (exp is WrapperExpression wrapperExpression)
            {
                return wrapperExpression;
            }

            throw new NotSupportedException(expression.ToString());
        }
    }



}