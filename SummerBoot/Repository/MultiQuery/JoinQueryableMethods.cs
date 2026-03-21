using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace SummerBoot.Repository.MultiQuery;

public static class JoinQueryableMethods
{
    public static IJoinQueryable<T1, T2> LeftJoin<T1, T2>(
        IQueryable<T1> source,
        IQueryable<T2> second,
        Expression<Func<JoinCondition<T1, T2>, bool>> on)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinQueryable<T1, T2> RightJoin<T1, T2>(
        IQueryable<T1> source,
        IQueryable<T2> second,
        Expression<Func<JoinCondition<T1, T2>, bool>> on)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinQueryable<T1, T2> InnerJoin<T1, T2>(
        IQueryable<T1> source,
        IQueryable<T2> second,
        Expression<Func<JoinCondition<T1, T2>, bool>> on)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderQueryable<T1, T2> OrderBy<T1, T2, TKey>(IJoinQueryable<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderQueryable<T1, T2> OrderByDescending<T1, T2, TKey>(IJoinQueryable<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderQueryable<T1, T2> ThenBy<T1, T2, TKey>(IJoinOrderQueryable<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderQueryable<T1, T2> ThenByDescending<T1, T2, TKey>(IJoinOrderQueryable<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IEnumerable<TResult> Select<T1, T2, TResult>(IJoinOrderQueryable<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinQueryable<T1, T2> Where<T1, T2>(IJoinQueryable<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static int Count<T1, T2>(IJoinQueryable<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, bool>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinGroupQueryable<T1, T2, TKey> GroupBy<T1, T2, TKey>(IJoinQueryable<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IEnumerable<TResult> Select<T1, T2, TKey, TResult>(IJoinGroupQueryable<T1, T2, TKey> source, Expression<Func<IGrouping<TKey, JoinCondition<T1, T2>>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
}

internal static class JoinQueryableMethodsCache
{
    public static readonly MethodInfo LeftJoin =
        typeof(JoinQueryableMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinQueryableMethods.LeftJoin) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo RightJoin =
        typeof(JoinQueryableMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinQueryableMethods.RightJoin) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo InnerJoin =
        typeof(JoinQueryableMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinQueryableMethods.InnerJoin) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo OrderBy =
        typeof(JoinQueryableMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinQueryableMethods.OrderBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo ThenBy =
        typeof(JoinQueryableMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinQueryableMethods.ThenBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo OrderByDescending =
        typeof(JoinQueryableMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinQueryableMethods.OrderByDescending) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo ThenByDescending =
        typeof(JoinQueryableMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinQueryableMethods.ThenByDescending) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);


    public static readonly MethodInfo Select =
        typeof(JoinQueryableMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinQueryableMethods.Select) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo Where =
        typeof(JoinQueryableMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinQueryableMethods.Where) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo Count =
        typeof(JoinQueryableMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinQueryableMethods.Count) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo GroupBy =
        typeof(JoinQueryableMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinQueryableMethods.GroupBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo GroupBySelect =
        typeof(JoinQueryableMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinQueryableMethods.Select) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 4);
}
