using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SummerBoot.Repository
{
    public class Page<T> : IPage<T>
    {
        /// <summary>
        /// 分页总数
        /// </summary>
        public int TotalPages { set; get; }
        /// <summary>
        /// 当前页
        /// </summary>
        public int PageNumber { set; get; }
        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { set; get; }
        /// <summary>
        /// 返回结果值
        /// </summary>
        public List<T> Data { set; get; }
    }
}
