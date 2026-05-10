using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System;
using System.Threading.Tasks;

namespace SummerBoot.Repository.MultiQuery;

public class JoinRepository<T1> : IJoinRepository<T1>
{
    public ILambdaRepository<T1> Source { get; }

    public JoinRepository(ILambdaRepository<T1> source)
    {
        Source = source;
    }
    public IJoinRepository<T1, T2> LeftJoin<T2>(ILambdaRepository<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on)
    {
        var methodInfo = JoinRepositoryMethodsCache.LeftJoin.MakeGenericMethod(typeof(T1), typeof(T2));
        return InternalJoin(second, on, methodInfo);
    }

    public IJoinRepository<T1, T2> RightJoin<T2>(ILambdaRepository<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on)
    {
        var methodInfo = JoinRepositoryMethodsCache.RightJoin.MakeGenericMethod(typeof(T1), typeof(T2));
        return InternalJoin(second, on, methodInfo);
    }

    public IJoinRepository<T1, T2> InnerJoin<T2>(ILambdaRepository<T2> second, Expression<Func<JoinCondition<T1, T2>, bool>> on)
    {
        var methodInfo = JoinRepositoryMethodsCache.InnerJoin.MakeGenericMethod(typeof(T1), typeof(T2));
        return InternalJoin(second, on, methodInfo);
    }

    private IJoinRepository<T1, T2> InternalJoin<T2>(
        ILambdaRepository<T2> joinTable,
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

        var body = Source.Provider.CreateQuery<ILambdaRepository<JoinCondition<T1, T2>>>(callExpr);
        var result = new JoinRepository<T1, T2>(body);

        return result;
    }
}

public class JoinOrderRepository<T1, T2> : JoinRepository<T1, T2>, IJoinOrderRepository<T1, T2>
{
    public JoinOrderRepository(ILambdaRepository<JoinCondition<T1, T2>> source) : base(source)
    {
    }
    public IJoinOrderRepository<T1, T2> ThenBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        var methodInfo = JoinRepositoryMethodsCache.ThenBy.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }

    public IJoinOrderRepository<T1, T2> ThenByDescending<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        var methodInfo = JoinRepositoryMethodsCache.ThenByDescending.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }
}

public class JoinRepository<T1, T2> : IJoinRepository<T1, T2>
{
    public ILambdaRepository<JoinCondition<T1, T2>> Source { get; }
    private static MethodInfo leftJoinMethod = typeof(JoinRepository<T1, T2>).GetMethod(nameof(LeftJoin));
    private static MethodInfo rightJoinMethod = typeof(JoinRepository<T1, T2>).GetMethod(nameof(RightJoin));
    private static MethodInfo innerJoinMethod = typeof(JoinRepository<T1, T2>).GetMethod(nameof(InnerJoin));
    private static MethodInfo orderbyMethod = typeof(JoinRepository<T1, T2>).GetMethod(nameof(OrderBy));
    private static MethodInfo selectMethod = typeof(JoinRepository<T1, T2>).GetMethod(nameof(Select));
    public JoinRepository(ILambdaRepository<JoinCondition<T1, T2>> source)
    {
        Source = source;
    }

    public TResult Max<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        return default;
    }

    public TResult Min<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        return default;
    }
    public TResult Average<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        return default;
    }

    public TResult Sum<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        return default;
    }

    public IJoinGroupQueryable<T1, T2, TKey> GroupBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> selector)
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));
        var methodInfo = JoinRepositoryMethodsCache.GroupBy.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Source.Expression,
            Expression.Quote(selector)
        );
        var r = Source.Provider.CreateQuery<ILambdaRepository<JoinCondition<T1, T2>>>(callExpr);
        var result = new JoinGroupQueryable<T1, T2, TKey>(r);
        return result;
    }

    public IJoinRepository<T1, T2> Where(Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));
        var methodInfo = JoinRepositoryMethodsCache.Where.MakeGenericMethod(typeof(T1), typeof(T2));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Source.Expression,
            Expression.Quote(predicate)
        );

        var r = Source.Provider.CreateQuery<ILambdaRepository<JoinCondition<T1, T2>>>(callExpr);
        var result = new JoinRepository<T1, T2>(r);
        return result;
    }

    public IJoinRepository<T1, T2> WhereIf(bool condition, Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
    {
        if (condition)
        {
            return Where(predicate);
        }

        return this;
    }
    public IJoinOrderRepository<T1, T2> OrderBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        var methodInfo = JoinRepositoryMethodsCache.OrderBy.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }

    public IJoinOrderRepository<T1, T2> OrderByDescending<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        var methodInfo = JoinRepositoryMethodsCache.OrderByDescending.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }

    public IJoinRepository<T1, T2, T3> LeftJoin<T3>(ILambdaRepository<T3> second, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on)
    {
        var methodInfo = leftJoinMethod.MakeGenericMethod(typeof(T3));
        return InternalJoin(second, on, methodInfo);
    }

    public IJoinRepository<T1, T2, T3> RightJoin<T3>(ILambdaRepository<T3> second, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on)
    {
        var methodInfo = rightJoinMethod.MakeGenericMethod(typeof(T3));
        return InternalJoin(second, on, methodInfo);
    }

    public IJoinRepository<T1, T2, T3> InnerJoin<T3>(ILambdaRepository<T3> second, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on)
    {
        var methodInfo = innerJoinMethod.MakeGenericMethod(typeof(T3));
        return InternalJoin(second, on, methodInfo);
    }

    public ILambdaRepository<TResult> Select<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));

        var methodInfo = JoinRepositoryMethodsCache.Select.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TResult));
        var callExpr = Expression.Call(
       null,
       methodInfo,
       Source.Expression,
            Expression.Quote(selector)
        );

        var r = Source.Provider.CreateQuery<ILambdaRepository<TResult>>(callExpr);
        return r;
    }

    public int Count(Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));

        var methodInfo = JoinRepositoryMethodsCache.Count.MakeGenericMethod(typeof(T1), typeof(T2));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Source.Expression,
            Expression.Quote(predicate)
        );

        var result = Source.Provider.QueryFirstOrDefault<int>(callExpr);

        return result;
    }

    public async Task<int> CountAsync(Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));

        var methodInfo = JoinRepositoryMethodsCache.Count.MakeGenericMethod(typeof(T1), typeof(T2));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Source.Expression,
            Expression.Quote(predicate)
        );

        var result =await Source.Provider.QueryFirstOrDefaultAsync<int>(callExpr);

        return result;
    }

    protected IJoinOrderRepository<T1, T2> InternalOrderBy<TKey>(
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

        var r = Source.Provider.CreateQuery<ILambdaRepository<JoinCondition<T1, T2>>>(callExpr);
        var result = new JoinOrderRepository<T1, T2>(r);
        return result;
    }

    protected IJoinRepository<T1, T2, T3> InternalJoin<T3>(
        ILambdaRepository<T3> joinTable,
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
        var r = Source.Provider.CreateQuery<ILambdaRepository<JoinCondition<T1, T2, T3>>>(callExpr);
        var result = new JoinRepository<T1, T2, T3>(r);
        return result;
    }

}

public class JoinGroupQueryable<T1, T2, TKey> : IJoinGroupQueryable<T1, T2, TKey>
{
    public ILambdaRepository<JoinCondition<T1, T2>> Source { get; }

    public JoinGroupQueryable(ILambdaRepository<JoinCondition<T1, T2>> source)
    {
        Source = source;
    }
    public ILambdaRepository<TResult> Select<TResult>(Expression<Func<IGrouping<TKey, JoinCondition<T1, T2>>, TResult>> selector)
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));

        var methodInfo = JoinRepositoryMethodsCache.GroupBySelect.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey), typeof(TResult));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Source.Expression,
            Expression.Quote(selector)
        );

        var r = Source.Provider.CreateQuery<ILambdaRepository<TResult>>(callExpr);
        return r;
    }
}

public class JoinRepository<T1, T2, T3> : IJoinRepository<T1, T2, T3>
{
    public ILambdaRepository<JoinCondition<T1, T2, T3>> Source { get; }

    public JoinRepository(ILambdaRepository<JoinCondition<T1, T2, T3>> source)
    {
        Source = source;
    }
}