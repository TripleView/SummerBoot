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
        /// <summary>
        /// 生成经过分页的结果
        /// </summary>
        /// <returns></returns>
        Page<T> ToPage();
        /// <summary>
        /// 生成经过分页的结果
        /// </summary>
        /// <returns></returns>
        Task<Page<T>> ToPageAsync();
        /// <summary>
        /// 生成经过分页的结果
        /// </summary>
        /// <returns></returns>
        Page<T> ToPage(IPageable pageable);
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
    }
}