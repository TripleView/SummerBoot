using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System;

namespace SummerBoot.Repository.MultiQuery;

public class JoinQueryable<T1> : IJoinQueryable<T1>
{
    public IQueryable<T1> Source { get; }

    public JoinQueryable(IQueryable<T1> source)
    {
        Source = source;
    }
    public IJoinQueryable<T1, T2> LeftJoin<T2>(IQueryable<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on)
    {
        var methodInfo = JoinQueryableMethodsCache.LeftJoin.MakeGenericMethod(typeof(T1), typeof(T2));
        return InternalJoin(second, on, methodInfo);
    }

    public IJoinQueryable<T1, T2> RightJoin<T2>(IQueryable<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on)
    {
        var methodInfo = JoinQueryableMethodsCache.RightJoin.MakeGenericMethod(typeof(T1), typeof(T2));
        return InternalJoin(second, on, methodInfo);
    }

    public IJoinQueryable<T1, T2> InnerJoin<T2>(IQueryable<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on)
    {
        var methodInfo = JoinQueryableMethodsCache.InnerJoin.MakeGenericMethod(typeof(T1), typeof(T2));
        return InternalJoin(second, on, methodInfo);
    }

    private IJoinQueryable<T1, T2> InternalJoin<T2>(
        IQueryable<T2> joinTable,
        Expression<Func<JoinCondition<T1, T2>, bool>> on,
        MethodInfo methodInfo
    )
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));
        if (joinTable == null) throw new ArgumentNullException(nameof(joinTable));
        if (on == null) throw new ArgumentNullException(nameof(on));

        var callExpr = Expression.Call(
            null,
            methodInfo,
            Source.Expression,
            joinTable.Expression,
            Expression.Quote(on));

        var body = Source.Provider.CreateQuery<JoinCondition<T1, T2>>(callExpr);
        var result = new JoinQueryable<T1, T2>(body);

        return result;
    }
}

public class JoinOrderQueryable<T1, T2> : JoinQueryable<T1, T2>, IJoinOrderQueryable<T1, T2>
{
    public JoinOrderQueryable(IQueryable<JoinCondition<T1, T2>> source) : base(source)
    {
    }
    public IJoinOrderQueryable<T1, T2> ThenBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        var methodInfo = JoinQueryableMethodsCache.ThenBy.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }

    public IJoinOrderQueryable<T1, T2> ThenByDescending<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        var methodInfo = JoinQueryableMethodsCache.ThenByDescending.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }
}

public class JoinQueryable<T1, T2> : IJoinQueryable<T1, T2>
{
    public IQueryable<JoinCondition<T1, T2>> Source { get; }
    private static MethodInfo leftJoinMethod = typeof(JoinQueryable<T1, T2>).GetMethod(nameof(LeftJoin));
    private static MethodInfo rightJoinMethod = typeof(JoinQueryable<T1, T2>).GetMethod(nameof(RightJoin));
    private static MethodInfo innerJoinMethod = typeof(JoinQueryable<T1, T2>).GetMethod(nameof(InnerJoin));
    private static MethodInfo orderbyMethod = typeof(JoinQueryable<T1, T2>).GetMethod(nameof(OrderBy));
    private static MethodInfo selectMethod = typeof(JoinQueryable<T1, T2>).GetMethod(nameof(Select));
    public JoinQueryable(IQueryable<JoinCondition<T1, T2>> source)
    {
        Source = source;
    }

    public IJoinQueryable<T1, T2> Where(Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));
        var methodInfo = JoinQueryableMethodsCache.Where.MakeGenericMethod(typeof(T1), typeof(T2));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Source.Expression,
            Expression.Quote(predicate)
        );

        var r = Source.Provider.CreateQuery<JoinCondition<T1, T2>>(callExpr);
        var result = new JoinQueryable<T1, T2>(r);
        return result;
    }

    public IJoinQueryable<T1, T2> WhereIf(bool condition, Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
    {
        if (condition)
        {
            return Where(predicate);
        }

        var result = new JoinQueryable<T1, T2>(Source);
        return result;
    }
    public IJoinOrderQueryable<T1, T2> OrderBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        var methodInfo = JoinQueryableMethodsCache.OrderBy.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }

    public IJoinOrderQueryable<T1, T2> OrderByDescending<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        var methodInfo = JoinQueryableMethodsCache.OrderByDescending.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }

    public IJoinQueryable<T1, T2, T3> LeftJoin<T3>(IQueryable<T3> second, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on)
    {
        var methodInfo = leftJoinMethod.MakeGenericMethod(typeof(T3));
        return InternalJoin(second, on, methodInfo);
    }

    public IJoinQueryable<T1, T2, T3> RightJoin<T3>(IQueryable<T3> second, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on)
    {
        var methodInfo = rightJoinMethod.MakeGenericMethod(typeof(T3));
        return InternalJoin(second, on, methodInfo);
    }

    public IJoinQueryable<T1, T2, T3> InnerJoin<T3>(IQueryable<T3> second, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on)
    {
        var methodInfo = innerJoinMethod.MakeGenericMethod(typeof(T3));
        return InternalJoin(second, on, methodInfo);
    }

    public IEnumerable<TResult> Select<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));

        var methodInfo = JoinQueryableMethodsCache.Select.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TResult));
        var callExpr = Expression.Call(
       null,
       methodInfo,
       Source.Expression,
            Expression.Quote(selector)
        );

        var r = Source.Provider.CreateQuery<TResult>(callExpr);
        return r;
    }

    protected IJoinOrderQueryable<T1, T2> InternalOrderBy<TKey>(
        Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector,
        MethodInfo methodInfo
    )
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));

        var callExpr = Expression.Call(
          null,
            methodInfo,
            Source.Expression,
            Expression.Quote(keySelector)
        );

        var r = Source.Provider.CreateQuery<JoinCondition<T1, T2>>(callExpr);
        var result = new JoinOrderQueryable<T1, T2>(r);
        return result;
    }

    protected IJoinQueryable<T1, T2, T3> InternalJoin<T3>(
        IQueryable<T3> joinTable,
        Expression<Func<JoinCondition<T1, T2, T3>, bool>> on,
        MethodInfo methodInfo
    )
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));
        if (joinTable == null) throw new ArgumentNullException(nameof(joinTable));
        if (on == null) throw new ArgumentNullException(nameof(on));

        var callExpr = Expression.Call(
            Expression.Constant(this),
            methodInfo,
            joinTable.Expression,
            Expression.Quote(on)
        );
        var r = Source.Provider.CreateQuery<JoinCondition<T1, T2, T3>>(callExpr);
        var result = new JoinQueryable<T1, T2, T3>(r);
        return result;
    }

}


public class JoinQueryable<T1, T2, T3> : IJoinQueryable<T1, T2, T3>
{
    public IQueryable<JoinCondition<T1, T2, T3>> Source { get; }

    //public IQueryable<JoinCondition<T1, T2, T3>> Source { get; }

    public JoinQueryable(IQueryable<JoinCondition<T1, T2, T3>> source)
    {
        Source = source;
    }
}