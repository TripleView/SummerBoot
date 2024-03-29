﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public interface IDbExecuteAndQuery
    {
      
        int InternalExecute(DbQueryResult param);
      
        Task<int> InternalExecuteAsync(DbQueryResult param);
       
        TResult InternalQuery<TResult>(DbQueryResult param);
        Task<TResult> InternalQueryAsync<TResult>(DbQueryResult param);
        List<TResult> InternalQueryList<TResult>(DbQueryResult param);

        Task<List<TResult>> InternalQueryListAsync<TResult>(DbQueryResult param);
        Page<TResult> InternalQueryPage<TResult>(DbQueryResult param);
       
        Task<Page<TResult>> InternalQueryPageAsync<TResult>(DbQueryResult param);
    }
}