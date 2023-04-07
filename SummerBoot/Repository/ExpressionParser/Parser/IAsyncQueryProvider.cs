using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public interface IAsyncQueryProvider : IQueryProvider
    {
        Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default);
    }
}
