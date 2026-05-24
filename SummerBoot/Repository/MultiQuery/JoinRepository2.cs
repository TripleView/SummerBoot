using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System;
using System.Threading.Tasks;

namespace SummerBoot.Repository.MultiQuery;

public class JoinOrderRepository<T1, T2> : JoinRepository<T1, T2>, IJoinOrderRepository<T1, T2>
{
    public JoinOrderRepository(ILambdaRepository<JoinCondition<T1, T2>> source) : base(source)
    {
    }
    public IJoinOrderRepository<T1, T2> ThenBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        var methodInfo = JoinRepository2MethodsCache.ThenBy.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }

    public IJoinOrderRepository<T1, T2> ThenByDescending<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        var methodInfo = JoinRepository2MethodsCache.ThenByDescending.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }
}

public class JoinRepository<T1, T2> : IJoinRepository<T1, T2>
{
    public ILambdaRepository<JoinCondition<T1, T2>> Source { get; }

    public JoinRepository(ILambdaRepository<JoinCondition<T1, T2>> source)
    {
        Source = source;
    }

    public TResult Max<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository2MethodsCache.Max);
        var result = Source.Provider.QueryFirstOrDefault<TResult>(callExpr);
        return result;
    }

    public TResult Min<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository2MethodsCache.Min);
        var result = Source.Provider.QueryFirstOrDefault<TResult>(callExpr);
        return result;
    }
    public TResult Average<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository2MethodsCache.Average);
        var result = Source.Provider.QueryFirstOrDefault<TResult>(callExpr);
        return result;
    }

    public TResult Sum<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository2MethodsCache.Sum);
        var result = Source.Provider.QueryFirstOrDefault<TResult>(callExpr);
        return result;
    }

    public async Task<TResult> MaxAsync<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository2MethodsCache.Max);
        var result = await Source.Provider.QueryFirstOrDefaultAsync<TResult>(callExpr);
        return result;
    }

    public async Task<TResult> MinAsync<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository2MethodsCache.Min);
        var result = await Source.Provider.QueryFirstOrDefaultAsync<TResult>(callExpr);
        return result;
    }
    public async Task<TResult> AverageAsync<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository2MethodsCache.Average);
        var result = await Source.Provider.QueryFirstOrDefaultAsync<TResult>(callExpr);
        return result;
    }

    public async Task<TResult> SumAsync<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository2MethodsCache.Sum);
        var result = await Source.Provider.QueryFirstOrDefaultAsync<TResult>(callExpr);
        return result;
    }

    private MethodCallExpression GetMaxOrMinOrSumOrAverageMethodCallExpression<TResult>(
        Expression<Func<JoinCondition<T1, T2>, TResult>> selector,
        MethodInfo methodInfo
    )
    {
        methodInfo = methodInfo.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TResult));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Source.Expression,
            Expression.Quote(selector)
        );
        return callExpr;
    }


    public IJoinGroupRepository<T1, T2, TKey> GroupBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> selector)
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));
        var methodInfo = JoinRepository2MethodsCache.GroupBy.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Source.Expression,
            Expression.Quote(selector)
        );
        var r = Source.Provider.CreateQuery<ILambdaRepository<JoinCondition<T1, T2>>>(callExpr);
        var result = new JoinGroupRepository<T1, T2, TKey>(r);
        return result;
    }

    public IJoinRepository<T1, T2> Where(Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));
        var methodInfo = JoinRepository2MethodsCache.Where.MakeGenericMethod(typeof(T1), typeof(T2));
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
        var methodInfo = JoinRepository2MethodsCache.OrderBy.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }

    public IJoinOrderRepository<T1, T2> OrderByDescending<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        var methodInfo = JoinRepository2MethodsCache.OrderByDescending.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }

    public IJoinRepository<T1, T2, T3> LeftJoin<T3>(ILambdaRepository<T3> second, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on)
    {
        var methodInfo = JoinRepository3MethodsCache.LeftJoin.MakeGenericMethod(typeof(T1), typeof(T2), typeof(T3));
        return InternalJoin(second, on, methodInfo);
    }

    public IJoinRepository<T1, T2, T3> RightJoin<T3>(ILambdaRepository<T3> second, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on)
    {
        var methodInfo = JoinRepository3MethodsCache.RightJoin.MakeGenericMethod(typeof(T1), typeof(T2), typeof(T3));
        return InternalJoin(second, on, methodInfo);
    }

    public IJoinRepository<T1, T2, T3> InnerJoin<T3>(ILambdaRepository<T3> second, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on)
    {
        var methodInfo = JoinRepository3MethodsCache.InnerJoin.MakeGenericMethod(typeof(T1), typeof(T2), typeof(T3));
        return InternalJoin(second, on, methodInfo);
    }

    public ILambdaRepository<TResult> Select<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));

        var methodInfo = JoinRepository2MethodsCache.Select.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TResult));
        var callExpr = Expression.Call(
       null,
       methodInfo,
       Source.Expression,
            Expression.Quote(selector)
        );

        var r = Source.Provider.CreateQuery<ILambdaRepository<TResult>>(callExpr);
        return r;
    }

    private MethodCallExpression GetCountMethodCallExpression(Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));

        var methodInfo = JoinRepository2MethodsCache.Count.MakeGenericMethod(typeof(T1), typeof(T2));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Source.Expression,
            Expression.Quote(predicate)
        );
        return callExpr;
    }

    public int Count(Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
    {
        var callExpr = GetCountMethodCallExpression(predicate);
        var result = Source.Provider.QueryFirstOrDefault<int>(callExpr);

        return result;
    }

    public async Task<int> CountAsync(Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
    {
        var callExpr = GetCountMethodCallExpression(predicate);
        var result = await Source.Provider.QueryFirstOrDefaultAsync<int>(callExpr);

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

public class JoinGroupRepository<T1, T2, TKey> : IJoinGroupRepository<T1, T2, TKey>
{
    public ILambdaRepository<JoinCondition<T1, T2>> Source { get; }

    public JoinGroupRepository(ILambdaRepository<JoinCondition<T1, T2>> source)
    {
        Source = source;
    }
    public ILambdaRepository<TResult> Select<TResult>(Expression<Func<IGrouping<TKey, JoinCondition<T1, T2>>, TResult>> selector)
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));

        var methodInfo = JoinRepository2MethodsCache.GroupBySelect.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey), typeof(TResult));
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