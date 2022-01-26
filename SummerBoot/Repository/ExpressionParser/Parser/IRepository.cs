using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public interface IRepository<T> : IOrderedQueryable<T>, IDbExecuteAndQuery
    {
        List<SelectItem<T>> SelectItems { set; get; }
        int ExecuteUpdate();
        Task<int> ExecuteUpdateAsync();
    }
}