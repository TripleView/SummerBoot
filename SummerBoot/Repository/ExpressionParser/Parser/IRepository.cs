using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using SummerBoot.Repository.ExpressionParser.Parser.MultiQuery;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public interface IRepository<T> : IOrderedQueryable<T>, IDbExecuteAndQuery,IAsyncQueryable<T>
    {
        List<SelectItem<T>> SelectItems { set; get; }
       
        List<JoinBodyBase<T>> JoinItems { set; get; }

        object MultiQuerySelectItem { set; get; }
        object MultiQuerySelectAutoFillItem { set; get; }

        object MultiQueryOrderByItem { set; get; }

        object MultiQueryWhereItem { set; get; }
        int ExecuteUpdate();
     
        /// <summary>
        /// 生成经过分页的结果
        /// </summary>
        /// <returns></returns>
        Page<T> ToPage();
       
        /// <summary>
        /// 生成经过分页的结果
        /// </summary>
        /// <returns></returns>
        Page<T> ToPage(IPageable pageable);


    }
}