using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SummerBoot.Repository
{
    public interface IPage<T>
    {
        /// <summary>
        /// 分页总数
        /// </summary>
        int TotalPages { set; get; }
        /// <summary>
        /// 当前页
        /// </summary>
        int PageNumber { set; get; }
        /// <summary>
        /// 每页数量
        /// </summary>
        int PageSize { set; get; }
        /// <summary>
        /// 返回结果值
        /// </summary>
        List<T> Data { set; get; }

       
    }
}
