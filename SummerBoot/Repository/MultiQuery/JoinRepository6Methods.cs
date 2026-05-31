using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace SummerBoot.Repository.MultiQuery;

public static class JoinRepository6Methods
{
    public static IJoinRepository<T1, T2, T3, T4, T5, T6> LeftJoin<T1, T2, T3, T4, T5, T6>(
        IJoinRepository<T1, T2, T3, T4, T5> source,
        IBaseRepository<T6> second,
        Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, bool>> on)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinRepository<T1, T2, T3, T4, T5, T6> RightJoin<T1, T2, T3, T4, T5, T6>(
        IJoinRepository<T1, T2, T3, T4, T5> source,
        IBaseRepository<T6> second,
        Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, bool>> on)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
    public static IJoinRepository<T1, T2, T3, T4, T5, T6> InnerJoin<T1, T2, T3, T4, T5, T6>(
        IJoinRepository<T1, T2, T3, T4, T5> source,
        IBaseRepository<T6> second,
        Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, bool>> on)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderRepository<T1, T2, T3, T4, T5, T6> OrderBy<T1, T2, T3, T4, T5, T6, TKey>(IJoinRepository<T1, T2, T3, T4, T5, T6> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderRepository<T1, T2, T3, T4, T5, T6> OrderByDescending<T1, T2, T3, T4, T5, T6, TKey>(IJoinRepository<T1, T2, T3, T4, T5, T6> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderRepository<T1, T2, T3, T4, T5, T6> ThenBy<T1, T2, T3, T4, T5, T6, TKey>(IJoinOrderRepository<T1, T2, T3, T4, T5, T6> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderRepository<T1, T2, T3, T4, T5, T6> ThenByDescending<T1, T2, T3, T4, T5, T6, TKey>(IJoinOrderRepository<T1, T2, T3, T4, T5, T6> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
    public static IJoinGroupRepository<T1, T2, T3, T4, T5, T6, TKey> GroupBy<T1, T2, T3, T4, T5, T6, TKey>(IJoinRepository<T1, T2, T3, T4, T5, T6> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
    public static ILambdaRepository<TResult> Select<T1, T2, T3, T4, T5, T6, TResult>(IJoinRepository<T1, T2, T3, T4, T5, T6> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinRepository<T1, T2, T3, T4, T5, T6> Where<T1, T2, T3, T4, T5, T6>(IJoinRepository<T1, T2, T3, T4, T5, T6> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, bool>> predicate)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
    public static TResult Max<T1, T2, T3, T4, T5, T6, TResult>(IJoinRepository<T1, T2, T3, T4, T5, T6> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static TResult Min<T1, T2, T3, T4, T5, T6, TResult>(IJoinRepository<T1, T2, T3, T4, T5, T6> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static TResult Sum<T1, T2, T3, T4, T5, T6, TResult>(IJoinRepository<T1, T2, T3, T4, T5, T6> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static TResult Average<T1, T2, T3, T4, T5, T6, TResult>(IJoinRepository<T1, T2, T3, T4, T5, T6> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static int Count<T1, T2, T3, T4, T5, T6>(IJoinRepository<T1, T2, T3, T4, T5, T6> source, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, bool>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IEnumerable<TResult> Select<T1, T2, T3, T4, T5, T6, TKey, TResult>(IJoinGroupRepository<T1, T2, T3, T4, T5, T6, TKey> source, Expression<Func<IGrouping<TKey, JoinCondition<T1, T2, T3, T4, T5, T6>>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
}

internal static class JoinRepository6MethodsCache
{
    public static readonly MethodInfo LeftJoin =
        typeof(JoinRepository6Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository6Methods.LeftJoin) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 6);

    public static readonly MethodInfo RightJoin =
        typeof(JoinRepository6Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository6Methods.RightJoin) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 6);

    public static readonly MethodInfo InnerJoin =
        typeof(JoinRepository6Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository6Methods.InnerJoin) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 6);

    public static readonly MethodInfo OrderBy =
        typeof(JoinRepository6Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository6Methods.OrderBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 7);

    public static readonly MethodInfo ThenBy =
        typeof(JoinRepository6Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository6Methods.ThenBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 7);

    public static readonly MethodInfo OrderByDescending =
        typeof(JoinRepository6Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository6Methods.OrderByDescending) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 7);

    public static readonly MethodInfo ThenByDescending =
        typeof(JoinRepository6Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository6Methods.ThenByDescending) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 7);


    public static readonly MethodInfo Select =
        typeof(JoinRepository6Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository6Methods.Select) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 7);

    public static readonly MethodInfo Where =
        typeof(JoinRepository6Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository6Methods.Where) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 6);

    public static readonly MethodInfo Count =
        typeof(JoinRepository6Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository6Methods.Count) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 6);

    public static readonly MethodInfo GroupBy =
        typeof(JoinRepository6Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository6Methods.GroupBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 7);

    public static readonly MethodInfo GroupBySelect =
        typeof(JoinRepository6Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository6Methods.Select) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 8);

    public static readonly MethodInfo Max =
        typeof(JoinRepository6Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository6Methods.Max) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 7);

    public static readonly MethodInfo Min =
        typeof(JoinRepository6Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository6Methods.Min) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 7);

    public static readonly MethodInfo Sum =
        typeof(JoinRepository6Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository6Methods.Sum) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 7);

    public static readonly MethodInfo Average =
        typeof(JoinRepository6Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository6Methods.Average) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 7);

}