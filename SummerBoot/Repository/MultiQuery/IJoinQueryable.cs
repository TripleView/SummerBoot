using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SummerBoot.Repository.MultiQuery;

public interface IJoinQueryable<T1>
{
    IJoinQueryable<T1, T2> LeftJoin<T2>(IQueryable<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on);
    IJoinQueryable<T1, T2> RightJoin<T2>(IQueryable<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on);
    IJoinQueryable<T1, T2> InnerJoin<T2>(IQueryable<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on);
}

public interface IJoinQueryable<T1, T2>
{
    IJoinQueryable<T1, T2, T3> LeftJoin<T3>(IQueryable<T3> table, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on);
    IJoinQueryable<T1, T2, T3> RightJoin<T3>(IQueryable<T3> table, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on);
    IJoinQueryable<T1, T2, T3> InnerJoin<T3>(IQueryable<T3> table, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on);
    IJoinOrderQueryable<T1, T2> OrderBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector);

    IJoinOrderQueryable<T1, T2> OrderByDescending<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector);

    IEnumerable<TResult> Select<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector);

    IJoinQueryable<T1, T2> Where(Expression<Func<JoinCondition<T1, T2>, bool>> predicate);

    IJoinQueryable<T1, T2> WhereIf(bool condition,Expression<Func<JoinCondition<T1, T2>, bool>> predicate);
}

public interface IJoinOrderQueryable<T1, T2> : IJoinQueryable<T1, T2>
{
    IJoinOrderQueryable<T1, T2> ThenBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector);

    IJoinOrderQueryable<T1, T2> ThenByDescending<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector);
}

public interface IJoinQueryable<T1, T2, T3>
{
    //IJoinQueryable<T1, T2> LeftJoin(IQueryable<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on);
    //IJoinQueryable<T1, T2> RightJoin(IQueryable<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on);
    //IJoinQueryable<T1, T2> InnerJoin(IQueryable<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on);
    //IJoinQueryable<T1, T2> OrderBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector);
    //IEnumerable<TResult> Select<TResult>(Func<(T1 T1, T2 T2), TResult> selector);
}



