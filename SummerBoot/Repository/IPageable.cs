using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SummerBoot.Repository
{
    public interface IPageable
    {
        /// <summary>
        /// 当前页
        /// </summary>
        int PageNumber { set; get; }
        /// <summary>
        /// 每页数量
        /// </summary>
        int PageSize { set; get; }
    }
}
