using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpressionParser.Parser
{
    public interface IDbExecuteAndQuery
    {
        int InternalExecute(DbQueryResult param);
        Task<int> InternalExecuteAsync(DbQueryResult param);
        TResult InternalQuery<TResult>(DbQueryResult param);

        List<TResult> InternalQueryList<TResult>(DbQueryResult param);
    }
}