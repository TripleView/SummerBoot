using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace SummerBoot.Repository.MultiQuery;

public static class JoinRepository3Methods
{
    public static IJoinRepository<T1, T2, T3> LeftJoin<T1, T2, T3>(
        IJoinRepository<T1, T2> source,
        IBaseRepository<T3> second,
        Expression<Func<JoinCondition<T1, T2, T3>, bool>> on)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinRepository<T1, T2, T3> RightJoin<T1, T2, T3>(
        IJoinRepository<T1, T2> source,
        IBaseRepository<T3> second,
        Expression<Func<JoinCondition<T1, T2, T3>, bool>> on)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
    public static IJoinRepository<T1, T2, T3> InnerJoin<T1, T2, T3>(
        IJoinRepository<T1, T2> source,
        IBaseRepository<T3> second,
        Expression<Func<JoinCondition<T1, T2, T3>, bool>> on)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
    
    public static IJoinOrderRepository<T1, T2, T3> OrderBy<T1, T2, T3, TKey>(IJoinRepository<T1, T2, T3> source, Expression<Func<JoinCondition<T1, T2, T3>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderRepository<T1, T2, T3> OrderByDescending<T1, T2, T3, TKey>(IJoinRepository<T1, T2, T3> source, Expression<Func<JoinCondition<T1, T2, T3>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderRepository<T1, T2, T3> ThenBy<T1, T2, T3, TKey>(IJoinOrderRepository<T1, T2, T3> source, Expression<Func<JoinCondition<T1, T2, T3>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderRepository<T1, T2, T3> ThenByDescending<T1, T2, T3, TKey>(IJoinOrderRepository<T1, T2, T3> source, Expression<Func<JoinCondition<T1, T2, T3>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
    public static IJoinGroupRepository<T1, T2, T3, TKey> GroupBy<T1, T2, T3, TKey>(IJoinRepository<T1, T2, T3> source, Expression<Func<JoinCondition<T1, T2, T3>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
    public static ILambdaRepository<TResult> Select<T1, T2, T3, TResult>(IJoinRepository<T1, T2, T3> source, Expression<Func<JoinCondition<T1, T2, T3>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinRepository<T1, T2, T3> Where<T1, T2, T3>(IJoinRepository<T1, T2, T3> source, Expression<Func<JoinCondition<T1, T2, T3>, bool>> predicate)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
    public static TResult Max<T1, T2, T3, TResult>(IJoinRepository<T1, T2, T3> source, Expression<Func<JoinCondition<T1, T2, T3>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static TResult Min<T1, T2, T3, TResult>(IJoinRepository<T1, T2, T3> source, Expression<Func<JoinCondition<T1, T2, T3>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static TResult Sum<T1, T2, T3, TResult>(IJoinRepository<T1, T2, T3> source, Expression<Func<JoinCondition<T1, T2, T3>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static TResult Average<T1, T2, T3, TResult>(IJoinRepository<T1, T2, T3> source, Expression<Func<JoinCondition<T1, T2, T3>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static int Count<T1, T2, T3>(IJoinRepository<T1, T2, T3> source, Expression<Func<JoinCondition<T1, T2, T3>, bool>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }



    public static IEnumerable<TResult> Select<T1, T2, T3, TKey, TResult>(IJoinGroupRepository<T1, T2, T3, TKey> source, Expression<Func<IGrouping<TKey, JoinCondition<T1, T2, T3>>, TResult>> selector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
}

internal static class JoinRepository3MethodsCache
{
    public static readonly MethodInfo LeftJoin =
        typeof(JoinRepository3Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository3Methods.LeftJoin) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo RightJoin =
        typeof(JoinRepository3Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository3Methods.RightJoin) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo InnerJoin =
        typeof(JoinRepository3Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository3Methods.InnerJoin) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo OrderBy =
        typeof(JoinRepository3Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository3Methods.OrderBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 4);

    public static readonly MethodInfo ThenBy =
        typeof(JoinRepository3Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository3Methods.ThenBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 4);

    public static readonly MethodInfo OrderByDescending =
        typeof(JoinRepository3Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository3Methods.OrderByDescending) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 4);

    public static readonly MethodInfo ThenByDescending =
        typeof(JoinRepository3Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository3Methods.ThenByDescending) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 4);


    public static readonly MethodInfo Select =
        typeof(JoinRepository3Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository3Methods.Select) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 4);

    public static readonly MethodInfo Where =
        typeof(JoinRepository3Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository3Methods.Where) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo Count =
        typeof(JoinRepository3Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository3Methods.Count) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 3);

    public static readonly MethodInfo GroupBy =
        typeof(JoinRepository3Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository3Methods.GroupBy) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length ==4);

    public static readonly MethodInfo GroupBySelect =
        typeof(JoinRepository3Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository3Methods.Select) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 5);

    public static readonly MethodInfo Max =
        typeof(JoinRepository3Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository3Methods.Max) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 4);

    public static readonly MethodInfo Min =
        typeof(JoinRepository3Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository3Methods.Min) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 4);

    public static readonly MethodInfo Sum =
        typeof(JoinRepository3Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository3Methods.Sum) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 4);

    public static readonly MethodInfo Average =
        typeof(JoinRepository3Methods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinRepository3Methods.Average) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length ==4);

}
