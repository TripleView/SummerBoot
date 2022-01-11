using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpressionParser.Parser
{
    public interface IDbExecuteAndQuery
    {
        int Execute(DbQueryResult param);
        Task<int> ExecuteAsync(DbQueryResult param);
        TResult Query<TResult>(DbQueryResult param);

        List<TResult> QueryList<TResult>(DbQueryResult param);
    }
}