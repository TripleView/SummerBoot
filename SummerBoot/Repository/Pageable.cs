using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SummerBoot.Repository
{
    public class Pageable : IPageable
    {
        public Pageable(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
        /// <summary>
        /// 当前页
        /// </summary>
        public int PageNumber { set; get; }
        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { set; get; }
    }
}
