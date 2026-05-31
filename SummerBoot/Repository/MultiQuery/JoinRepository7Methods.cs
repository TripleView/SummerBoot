using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace SummerBoot.Repository.MultiQuery;

public static class JoinRepository7Methods
{
    public static IJoinRepository<T1, T2, T3, T4, T5, T6, T7> LeftJoin<T1, T2, T3, T4, T5, T6, T7>(
        IJoinRepository<T1, T2, T3, T4, T5, T6> source,
        IBaseRepository<T7> second,
        Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6, T7>, bool>> on)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinRepository<T1, T2, T3, T4, T5, T6, T7> RightJoin<T1, T2, T3, T4, T5, T6, T7>(
        IJoinRepository<T1, T2, T3, T4, T5, T6> source,
        IBaseRepository<T7> second,
        Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6, T7>, bool>> on)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
    public static IJoinRepository<T1, T2, T3, T4, T5, T6, T7> InnerJoin<T1, T2, T3, T4, T5, T6, T7>(
        IJoinRepository<T1, T2, T3, T4, T5, T6> source,
        IBaseRepository<T7> second,
        Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6, T7>, bool>> on)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderRepository<T1, T2, T3, T4, T5, T6, T7> OrderBy<T1, T2, T3, T4, T5, T6, T7, TKey>(IJoinRepository<T1, T2, T3, T4, T5, T6, T7> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6, T7>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderRepository<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<T1, T2, T3, T4, T5, T6, T7, TKey>(IJoinRepository<T1, T2, T3, T4, T5, T6, T7> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6, T7>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderRepository<T1, T2, T3, T4, T5, T6, T7> ThenBy<T1, T2, T3, T4, T5, T6, T7, TKey>(IJoinOrderRepository<T1, T2, T3, T4, T5, T6, T7> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6, T7>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderRepository<T1, T2, T3, T4, T5, T6, T7> ThenByDescending<T1, T2, T3, T4, T5, T6, T7, TKey>(IJoinOrderRepository<T1, T2, T3, T4, T5, T6, T7> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6, T7>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
    public static IJoinGroupRepository<T1, T2, T3, T4, T5, T6, T7, TKey> GroupBy<T1, T2, T3, T4, T5, T6, T7, TKey>(IJoinRepository<T1, T2, T3, T4, T5, T6, T7> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6, T7>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
    public static ILambdaRepository<TResult> Select<T1, T2, T3, T4, T5, T6, T7, TResult>(IJoinRepository<T1, T2, T3, T4, T5, T6, T7> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6, T7>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinRepository<T1, T2, T3, T4, T5, T6, T7> Where<T1, T2, T3, T4, T5, T6, T7>(IJoinRepository<T1, T2, T3, T4, T5, T6, T7> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6, T7>, bool>> predicate)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
    public static TResult Max<T1, T2, T3, T4, T5, T6, T7, TResult>(IJoinRepository<T1, T2, T3, T4, T5, T6, T7> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6, T7>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static TResult Min<T1, T2, T3, T4, T5, T6, T7, TResult>(IJoinRepository<T1, T2, T3, T4, T5, T6, T7> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6, T7>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static TResult Sum<T1, T2, T3, T4, T5, T6, T7, TResult>(IJoinRepository<T1, T2, T3, T4, T5, T6, T7> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6, T7>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static TResult Average<T1, T2, T3, T4, T5, T6, T7, TResult>(IJoinRepository<T1, T2, T3, T4, T5, T6, T7> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6, T7>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static int Count<T1, T2, T3, T4, T5, T6, T7>(IJoinRepository<T1, T2, T3, T4, T5, T6, T7> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6, T7>, bool>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IEnumerable<TResult> Select<T1, T2, T3, T4, T5, T6, T7, TKey, TResult>(IJoinGroupRepository<T1, T2, T3, T4, T5, T6, T7, TKey> source, Expression<Func<IGrouping<TKey, JoinCondition<T1, T2, T3, T4, T5, T6, T7>>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
}

internal static class JoinRepository7MethodsCache
{
    public static readonly MethodInfo LeftJoin =
        typeof(JoinRepository7Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository7Methods.LeftJoin) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 7);

    public static readonly MethodInfo RightJoin =
        typeof(JoinRepository7Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository7Methods.RightJoin) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 7);

    public static readonly MethodInfo InnerJoin =
        typeof(JoinRepository7Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository7Methods.InnerJoin) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 7);

    public static readonly MethodInfo OrderBy =
        typeof(JoinRepository7Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository7Methods.OrderBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 8);

    public static readonly MethodInfo ThenBy =
        typeof(JoinRepository7Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository7Methods.ThenBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 8);

    public static readonly MethodInfo OrderByDescending =
        typeof(JoinRepository7Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository7Methods.OrderByDescending) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 8);

    public static readonly MethodInfo ThenByDescending =
        typeof(JoinRepository7Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository7Methods.ThenByDescending) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 8);


    public static readonly MethodInfo Select =
        typeof(JoinRepository7Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository7Methods.Select) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 8);

    public static readonly MethodInfo Where =
        typeof(JoinRepository7Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository7Methods.Where) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 7);

    public static readonly MethodInfo Count =
        typeof(JoinRepository7Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository7Methods.Count) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 7);

    public static readonly MethodInfo GroupBy =
        typeof(JoinRepository7Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository7Methods.GroupBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 8);

    public static readonly MethodInfo GroupBySelect =
        typeof(JoinRepository7Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository7Methods.Select) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 9);

    public static readonly MethodInfo Max =
        typeof(JoinRepository7Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository7Methods.Max) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 8);

    public static readonly MethodInfo Min =
        typeof(JoinRepository7Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository7Methods.Min) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 8);

    public static readonly MethodInfo Sum =
        typeof(JoinRepository7Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository7Methods.Sum) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 8);

    public static readonly MethodInfo Average =
        typeof(JoinRepository7Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository7Methods.Average) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 8);

}