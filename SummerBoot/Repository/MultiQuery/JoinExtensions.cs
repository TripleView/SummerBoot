using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System;

namespace SummerBoot.Repository.MultiQuery;

public static class JoinExtensions
{
    public static IQueryable<JoinTuple<T1, T2>> LeftJoin<T1, T2>(
        this IQueryable<T1> source,
        IQueryable<T2> joinTable,
        Expression<Func<JoinTuple<T1, T2>, bool>> on) where T1 : new() where T2 : new()
    {
        return source.LeftJoin(new T2(), on);
    }

    public static IQueryable<JoinTuple<T1, T2>> LeftJoin<T1, T2>(
        this IQueryable<T1> source,
        T2 joinTable,
        Expression<Func<JoinTuple<T1, T2>, bool>> on) where T1 : new() where T2 : new()
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (joinTable == null) throw new ArgumentNullException(nameof(joinTable));
        if (on == null) throw new ArgumentNullException(nameof(on));

        // 构造 LeftJoin 的表达式树
        var leftJoinMethod = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T1), typeof(T2));
        var callExpr = Expression.Call(
            null,
            leftJoinMethod,
            source.Expression,
            Expression.Constant(typeof(T2)),
            Expression.Quote(on)
        );

        // 让 source.Provider 创建新的 IQueryable<JoinTuple<T1, T2>>
        return source.Provider.CreateQuery<JoinTuple<T1, T2>>(callExpr);
    }
}