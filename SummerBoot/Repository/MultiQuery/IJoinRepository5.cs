using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SummerBoot.Repository.MultiQuery;

public interface IJoinRepository<T1, T2, T3, T4, T5>
{
    IJoinRepository<T1, T2, T3, T4, T5, T6> LeftJoin<T6>(ILambdaRepository<T6> table, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, bool>> on);
    IJoinRepository<T1, T2, T3, T4, T5, T6> RightJoin<T6>(ILambdaRepository<T6> table, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, bool>> on);
    IJoinRepository<T1, T2, T3, T4, T5, T6> InnerJoin<T6>(ILambdaRepository<T6> table, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, bool>> on);
    IJoinOrderRepository<T1, T2, T3, T4, T5> OrderBy<TKey>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TKey>> keySelector);

    IJoinOrderRepository<T1, T2, T3, T4, T5> OrderByDescending<TKey>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TKey>> keySelector);

    ILambdaRepository<TResult> Select<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector);

    IJoinRepository<T1, T2, T3, T4, T5> Where(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, bool>> predicate);

    IJoinRepository<T1, T2, T3, T4, T5> WhereIf(bool condition, Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, bool>> predicate);

    int Count(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, bool>> predicate);

    Task<int> CountAsync(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, bool>> predicate);

    TResult Max<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector);

    TResult Min<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector);
    TResult Average<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector);

    TResult Sum<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector);


    Task<TResult> MaxAsync<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector);

    Task<TResult> MinAsync<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector);
    Task<TResult> AverageAsync<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector);

    Task<TResult> SumAsync<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector);

    IJoinGroupRepository<T1, T2, T3, T4, T5, TKey> GroupBy<TKey>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TKey>> selector);
}

public interface IJoinGroupRepository<T1, T2, T3, T4, T5, TKey>
{
    ILambdaRepository<TResult> Select<TResult>(Expression<Func<IGrouping<TKey, JoinCondition<T1, T2, T3, T4, T5>>, TResult>> selector);
}


public interface IJoinOrderRepository<T1, T2, T3, T4, T5> : IJoinRepository<T1, T2, T3, T4, T5>
{
    IJoinOrderRepository<T1, T2, T3, T4, T5> ThenBy<TKey>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TKey>> keySelector);

    IJoinOrderRepository<T1, T2, T3, T4, T5> ThenByDescending<TKey>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TKey>> keySelector);
}



