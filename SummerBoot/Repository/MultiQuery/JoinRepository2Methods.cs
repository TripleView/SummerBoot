using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace SummerBoot.Repository.MultiQuery;

public static class JoinRepository2Methods
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
    public static IJoinGroupRepository<T1, T2, TKey> GroupBy<T1, T2, TKey>(IJoinRepository<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
    public static ILambdaRepository<TResult> Select<T1, T2, TResult>(IJoinRepository<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinRepository<T1, T2> Where<T1, T2>(IJoinRepository<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
    public static TResult Max<T1, T2, TResult>(IJoinRepository<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static TResult Min<T1, T2, TResult>(IJoinRepository<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static TResult Sum<T1, T2, TResult>(IJoinRepository<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static TResult Average<T1, T2, TResult>(IJoinRepository<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static int Count<T1, T2>(IJoinRepository<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, bool>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }



    public static IEnumerable<TResult> Select<T1, T2, TKey, TResult>(IJoinGroupRepository<T1, T2, TKey> source, Expression<Func<IGrouping<TKey, JoinCondition<T1, T2>>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
}

internal static class JoinRepository2MethodsCache
{
    public static readonly MethodInfo LeftJoin =
        typeof(JoinRepository2Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository2Methods.LeftJoin) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo RightJoin =
        typeof(JoinRepository2Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository2Methods.RightJoin) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo InnerJoin =
        typeof(JoinRepository2Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository2Methods.InnerJoin) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo OrderBy =
        typeof(JoinRepository2Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository2Methods.OrderBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo ThenBy =
        typeof(JoinRepository2Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository2Methods.ThenBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo OrderByDescending =
        typeof(JoinRepository2Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository2Methods.OrderByDescending) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo ThenByDescending =
        typeof(JoinRepository2Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository2Methods.ThenByDescending) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);


    public static readonly MethodInfo Select =
        typeof(JoinRepository2Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository2Methods.Select) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo Where =
        typeof(JoinRepository2Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository2Methods.Where) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo Count =
        typeof(JoinRepository2Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository2Methods.Count) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

    public static readonly MethodInfo GroupBy =
        typeof(JoinRepository2Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository2Methods.GroupBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo GroupBySelect =
        typeof(JoinRepository2Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository2Methods.Select) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 4);

    public static readonly MethodInfo Max =
        typeof(JoinRepository2Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository2Methods.Max) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo Min =
        typeof(JoinRepository2Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository2Methods.Min) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo Sum =
        typeof(JoinRepository2Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository2Methods.Sum) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo Average =
        typeof(JoinRepository2Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository2Methods.Average) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

}
