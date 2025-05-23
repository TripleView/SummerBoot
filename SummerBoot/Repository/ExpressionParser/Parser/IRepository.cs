﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using SummerBoot.Repository.ExpressionParser.Parser.MultiQuery;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public interface IRepository<T> : IOrderedQueryable<T>, IDbExecuteAndQuery, IAsyncQueryable<T>
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        List<SelectItem<T>> SelectItems { set; get; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        MultiQueryContext<T> MultiQueryContext { set; get; }
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

        //IQueryable<T> OrWhere(Expression<Predicate<T>> predicate);

    }
}