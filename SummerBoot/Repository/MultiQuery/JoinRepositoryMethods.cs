using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace SummerBoot.Repository.MultiQuery;

public static class JoinRepositoryMethods
{
    public static IJoinRepository<T1, T2> LeftJoin<T1, T2>(
        IBaseRepository<T1> source,
        IBaseRepository<T2> second,
        Expression<Func<JoinCondition<T1, T2>, bool>> on)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinRepository<T1, T2> RightJoin<T1, T2>(
        IBaseRepository<T1> source,
        IBaseRepository<T2> second,
        Expression<Func<JoinCondition<T1, T2>, bool>> on)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinRepository<T1, T2> InnerJoin<T1, T2>(
        IBaseRepository<T1> source,
        IBaseRepository<T2> second,
        Expression<Func<JoinCondition<T1, T2>, bool>> on)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderRepository<T1, T2> OrderBy<T1, T2, TKey>(IJoinRepository<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderRepository<T1, T2> OrderByDescending<T1, T2, TKey>(IJoinRepository<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderRepository<T1, T2> ThenBy<T1, T2, TKey>(IJoinOrderRepository<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderRepository<T1, T2> ThenByDescending<T1, T2, TKey>(IJoinOrderRepository<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IEnumerable<TResult> Select<T1, T2, TResult>(IJoinOrderRepository<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinRepository<T1, T2> Where<T1, T2>(IJoinRepository<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static int Count<T1, T2>(IJoinRepository<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, bool>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinGroupQueryable<T1, T2, TKey> GroupBy<T1, T2, TKey>(IJoinRepository<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IEnumerable<TResult> Select<T1, T2, TKey, TResult>(IJoinGroupQueryable<T1, T2, TKey> source, Expression<Func<IGrouping<TKey, JoinCondition<T1, T2>>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
}

internal static class JoinRepositoryMethodsCache
{
    public static readonly MethodInfo LeftJoin =
        typeof(JoinRepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepositoryMethods.LeftJoin) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo RightJoin =
        typeof(JoinRepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepositoryMethods.RightJoin) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo InnerJoin =
        typeof(JoinRepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepositoryMethods.InnerJoin) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo OrderBy =
        typeof(JoinRepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepositoryMethods.OrderBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo ThenBy =
        typeof(JoinRepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepositoryMethods.ThenBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo OrderByDescending =
        typeof(JoinRepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepositoryMethods.OrderByDescending) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo ThenByDescending =
        typeof(JoinRepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepositoryMethods.ThenByDescending) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);


    public static readonly MethodInfo Select =
        typeof(JoinRepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepositoryMethods.Select) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo Where =
        typeof(JoinRepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepositoryMethods.Where) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo Count =
        typeof(JoinRepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepositoryMethods.Count) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo GroupBy =
        typeof(JoinRepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepositoryMethods.GroupBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo GroupBySelect =
        typeof(JoinRepositoryMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepositoryMethods.Select) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 4);
}
