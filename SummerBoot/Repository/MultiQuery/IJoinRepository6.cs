using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SummerBoot.Repository.MultiQuery;

public interface IJoinRepository<T1, T2, T3, T4, T5, T6>
{
    IJoinRepository<T1, T2, T3, T4, T5, T6, T7> LeftJoin<T7>(ILambdaRepository<T7> table, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6, T7>, bool>> on);
    IJoinRepository<T1, T2, T3, T4, T5, T6, T7> RightJoin<T7>(ILambdaRepository<T7> table, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6, T7>, bool>> on);
    IJoinRepository<T1, T2, T3, T4, T5, T6, T7> InnerJoin<T7>(ILambdaRepository<T7> table, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6, T7>, bool>> on);
    IJoinOrderRepository<T1, T2, T3, T4, T5, T6> OrderBy<TKey>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TKey>> keySelector);

    IJoinOrderRepository<T1, T2, T3, T4, T5, T6> OrderByDescending<TKey>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TKey>> keySelector);

    ILambdaRepository<TResult> Select<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TResult>> selector);

    IJoinRepository<T1, T2, T3, T4, T5, T6> Where(Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, bool>> predicate);

    IJoinRepository<T1, T2, T3, T4, T5, T6> WhereIf(bool condition, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, bool>> predicate);

    int Count(Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, bool>> predicate);

    Task<int> CountAsync(Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, bool>> predicate);

    TResult Max<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TResult>> selector);

    TResult Min<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TResult>> selector);
    TResult Average<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TResult>> selector);

    TResult Sum<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TResult>> selector);


    Task<TResult> MaxAsync<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TResult>> selector);

    Task<TResult> MinAsync<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TResult>> selector);
    Task<TResult> AverageAsync<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TResult>> selector);

    Task<TResult> SumAsync<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TResult>> selector);

    IJoinGroupRepository<T1, T2, T3, T4, T5, T6, TKey> GroupBy<TKey>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TKey>> selector);
}

public interface IJoinGroupRepository<T1, T2, T3, T4, T5, T6, TKey>
{
    ILambdaRepository<TResult> Select<TResult>(Expression<Func<IGrouping<TKey, JoinCondition<T1, T2, T3, T4, T5, T6>>, TResult>> selector);
}

public interface IJoinOrderRepository<T1, T2, T3, T4, T5, T6> : IJoinRepository<T1, T2, T3, T4, T5, T6>
{
    IJoinOrderRepository<T1, T2, T3, T4, T5, T6> ThenBy<TKey>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TKey>> keySelector);

    IJoinOrderRepository<T1, T2, T3, T4, T5, T6> ThenByDescending<TKey>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TKey>> keySelector);
}



