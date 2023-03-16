using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public interface IAsyncQueryable<T>
    {

        Task<int> ExecuteUpdateAsync();
        /// <summary>
        /// 生成经过分页的结果
        /// </summary>
        /// <returns></returns>
        Task<Page<T>> ToPageAsync();
        /// <summary>
        /// 生成经过分页的结果
        /// </summary>
        /// <returns></returns>
        Task<Page<T>> ToPageAsync(IPageable pageable);
        /// <summary>
        /// 生成经过分页的结果
        /// </summary>
        /// <returns></returns>
        Task<List<T>> ToListAsync();
        /// <summary>
        /// 获取第一个或者默认的值
        /// </summary>
        /// <returns></returns>
        Task<T> FirstOrDefaultAsync();
        /// <summary>
        /// 获取第一个或者默认的值
        /// </summary>
        /// <returns></returns>
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> selector);
        /// <summary>
        /// 获取第一个的值
        /// </summary>
        /// <returns></returns>
        Task<T> FirstAsync();
        Task<T> FirstAsync(Expression<Func<T, bool>> selector);
        Task<TResult> MaxAsync<TResult>(
            Expression<Func<T, TResult>> selector);

        Task<TResult> MinAsync<TResult>(
            Expression<Func<T, TResult>> selector);

        Task<TResult> SumAsync<TResult>(
            Expression<Func<T, TResult>> selector);

        Task<TResult> AverageAsync<TResult>(
            Expression<Func<T, TResult>> selector);

        Task<int> CountAsync(
            Expression<Func<T, bool>> selector);

    }
}
