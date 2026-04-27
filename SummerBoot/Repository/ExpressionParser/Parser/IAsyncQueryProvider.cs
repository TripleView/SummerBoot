using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public interface IAsyncQueryProvider : IQueryProvider
    {
        Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default);
        /// <summary>
        /// 执行删除或者更新
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> ExecuteDeleteOrUpdateAsync(Expression expression, CancellationToken cancellationToken = default);
        /// <summary>
        /// 执行删除或者更新
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        int ExecuteDeleteOrUpdate(Expression expression, CancellationToken cancellationToken = default);
    }
}
