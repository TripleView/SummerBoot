using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public interface IDbExecuteAndQuery
    {

        [EditorBrowsable(EditorBrowsableState.Never)]
        int InternalExecute(DbQueryResult param);

        [EditorBrowsable(EditorBrowsableState.Never)]
        Task<int> InternalExecuteAsync(DbQueryResult param);

        [EditorBrowsable(EditorBrowsableState.Never)]
        TResult InternalQuery<TResult>(DbQueryResult param);

        [EditorBrowsable(EditorBrowsableState.Never)]
        Task<TResult> InternalQueryAsync<TResult>(DbQueryResult param);

        [EditorBrowsable(EditorBrowsableState.Never)]
        List<TResult> InternalQueryList<TResult>(DbQueryResult param);

        [EditorBrowsable(EditorBrowsableState.Never)]
        Task<List<TResult>> InternalQueryListAsync<TResult>(DbQueryResult param);

        [EditorBrowsable(EditorBrowsableState.Never)]
        Page<TResult> InternalQueryPage<TResult>(DbQueryResult param);

        [EditorBrowsable(EditorBrowsableState.Never)]
        Task<Page<TResult>> InternalQueryPageAsync<TResult>(DbQueryResult param);
    }
}