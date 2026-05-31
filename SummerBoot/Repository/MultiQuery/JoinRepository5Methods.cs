using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace SummerBoot.Repository.MultiQuery;

public static class JoinRepository5Methods
{
    public static IJoinRepository<T1, T2, T3, T4, T5> LeftJoin<T1, T2, T3, T4, T5>(
        IJoinRepository<T1, T2, T3, T4> source,
        IBaseRepository<T5> second,
        Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, bool>> on)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinRepository<T1, T2, T3, T4, T5> RightJoin<T1, T2, T3, T4, T5>(
        IJoinRepository<T1, T2, T3, T4> source,
        IBaseRepository<T5> second,
        Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, bool>> on)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
    public static IJoinRepository<T1, T2, T3, T4, T5> InnerJoin<T1, T2, T3, T4, T5>(
        IJoinRepository<T1, T2, T3, T4> source,
        IBaseRepository<T5> second,
        Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, bool>> on)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderRepository<T1, T2, T3, T4, T5> OrderBy<T1, T2, T3, T4, T5, TKey>(IJoinRepository<T1, T2, T3, T4, T5> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderRepository<T1, T2, T3, T4, T5> OrderByDescending<T1, T2, T3, T4, T5, TKey>(IJoinRepository<T1, T2, T3, T4, T5> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderRepository<T1, T2, T3, T4, T5> ThenBy<T1, T2, T3, T4, T5, TKey>(IJoinOrderRepository<T1, T2, T3, T4, T5> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderRepository<T1, T2, T3, T4, T5> ThenByDescending<T1, T2, T3, T4, T5, TKey>(IJoinOrderRepository<T1, T2, T3, T4, T5> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
    public static IJoinGroupRepository<T1, T2, T3, T4, T5, TKey> GroupBy<T1, T2, T3, T4, T5, TKey>(IJoinRepository<T1, T2, T3, T4, T5> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
    public static ILambdaRepository<TResult> Select<T1, T2, T3, T4, T5, TResult>(IJoinRepository<T1, T2, T3, T4, T5> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinRepository<T1, T2, T3, T4, T5> Where<T1, T2, T3, T4, T5>(IJoinRepository<T1, T2, T3, T4, T5> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, bool>> predicate)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
    public static TResult Max<T1, T2, T3, T4, T5, TResult>(IJoinRepository<T1, T2, T3, T4, T5> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static TResult Min<T1, T2, T3, T4, T5, TResult>(IJoinRepository<T1, T2, T3, T4, T5> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static TResult Sum<T1, T2, T3, T4, T5, TResult>(IJoinRepository<T1, T2, T3, T4, T5> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static TResult Average<T1, T2, T3, T4, T5, TResult>(IJoinRepository<T1, T2, T3, T4, T5> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static int Count<T1, T2, T3, T4, T5>(IJoinRepository<T1, T2, T3, T4, T5> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, bool>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IEnumerable<TResult> Select<T1, T2, T3, T4, T5, TKey, TResult>(IJoinGroupRepository<T1, T2, T3, T4, T5, TKey> source, Expression<Func<IGrouping<TKey, JoinCondition<T1, T2, T3, T4, T5>>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
}

internal static class JoinRepository5MethodsCache
{
    public static readonly MethodInfo LeftJoin =
        typeof(JoinRepository5Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository5Methods.LeftJoin) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 5);

    public static readonly MethodInfo RightJoin =
        typeof(JoinRepository5Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository5Methods.RightJoin) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 5);

    public static readonly MethodInfo InnerJoin =
        typeof(JoinRepository5Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository5Methods.InnerJoin) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 5);

    public static readonly MethodInfo OrderBy =
        typeof(JoinRepository5Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository5Methods.OrderBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 6);

    public static readonly MethodInfo ThenBy =
        typeof(JoinRepository5Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository5Methods.ThenBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 6);

    public static readonly MethodInfo OrderByDescending =
        typeof(JoinRepository5Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository5Methods.OrderByDescending) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 6);

    public static readonly MethodInfo ThenByDescending =
        typeof(JoinRepository5Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository5Methods.ThenByDescending) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 6);


    public static readonly MethodInfo Select =
        typeof(JoinRepository5Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository5Methods.Select) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 6);

    public static readonly MethodInfo Where =
        typeof(JoinRepository5Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository5Methods.Where) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 5);

    public static readonly MethodInfo Count =
        typeof(JoinRepository5Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository5Methods.Count) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 5);

    public static readonly MethodInfo GroupBy =
        typeof(JoinRepository5Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository5Methods.GroupBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 6);

    public static readonly MethodInfo GroupBySelect =
        typeof(JoinRepository5Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository5Methods.Select) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 7);

    public static readonly MethodInfo Max =
        typeof(JoinRepository5Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository5Methods.Max) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 6);

    public static readonly MethodInfo Min =
        typeof(JoinRepository5Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository5Methods.Min) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 6);

    public static readonly MethodInfo Sum =
        typeof(JoinRepository5Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository5Methods.Sum) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 6);

    public static readonly MethodInfo Average =
        typeof(JoinRepository5Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository5Methods.Average) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 6);

}