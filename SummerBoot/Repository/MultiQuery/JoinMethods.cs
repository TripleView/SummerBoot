using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System;

namespace SummerBoot.Repository.MultiQuery;

public static class JoinQueryableMethods
{
    public static IQueryable<JoinCondition<T1, T2>> LeftJoin<T1, T2>(
        this IQueryable<T1> source,
        IQueryable<T2> second,
        Expression<Func<JoinCondition<T1, T2>, bool>> on)
    {
        throw new NotSupportedException("Only for expression translation.");
    }

    public static IJoinOrderQueryable<T1, T2> OrderBy<T1, T2, TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        throw new NotSupportedException("Only for expression translation.");
    }
}

internal static class MethodCache
{
    // 打开的泛型方法定义，只做一次反射查找
    public static readonly MethodInfo LeftJoin =
        typeof(JoinQueryableMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinQueryableMethods.LeftJoin) && m.IsGenericMethodDefinition);

    public static readonly MethodInfo OrderBy =
        typeof(JoinQueryableMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(JoinQueryableMethods.OrderBy) && m.IsGenericMethodDefinition);
}

// 利用泛型静态类对“闭合方法”按 (T1,T2) 组合缓存，JIT 级别一次性初始化，线程安全
internal static class JoinQueryableMethodsCache<T1, T2>
{
    public static readonly MethodInfo LeftJoinMethod =
        MethodCache.LeftJoin.MakeGenericMethod(typeof(T1), typeof(T2));

}