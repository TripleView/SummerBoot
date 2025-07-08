using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SummerBoot.Repository
{
    public interface IPageable
    {
        /// <summary>
        /// Current Page
        /// 当前页
        /// </summary>
        int PageNumber { set; get; }
        /// <summary>
        /// Quantity per page
        /// 每页数量
        /// </summary>
        int PageSize { set; get; }
        /// <summary>
        /// Sorting rules list
        /// 排序规则列表
        /// </summary>
        List<OrderByItem> OrderByItems { set; get; }
    }
}
