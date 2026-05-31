using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System;
using System.Threading.Tasks;

namespace SummerBoot.Repository.MultiQuery;


public class JoinOrderRepository<T1, T2, T3, T4, T5> : JoinRepository<T1, T2, T3, T4, T5>, IJoinOrderRepository<T1, T2, T3, T4, T5>
{
    public JoinOrderRepository(ILambdaRepository<JoinCondition<T1, T2, T3, T4, T5>> source) : base(source)
    {
    }
    public IJoinOrderRepository<T1, T2, T3, T4, T5> ThenBy<TKey>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TKey>> keySelector)
    {
        var methodInfo = JoinRepository5MethodsCache.ThenBy.MakeGenericMethod(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }

    public IJoinOrderRepository<T1, T2, T3, T4, T5> ThenByDescending<TKey>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TKey>> keySelector)
    {
        var methodInfo = JoinRepository5MethodsCache.ThenByDescending.MakeGenericMethod(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }
}

public class JoinRepository<T1, T2, T3, T4, T5> : IJoinRepository<T1, T2, T3, T4, T5>
{
    public ILambdaRepository<JoinCondition<T1, T2, T3, T4, T5>> Source { get; }

    public JoinRepository(ILambdaRepository<JoinCondition<T1, T2, T3, T4, T5>> source)
    {
        Source = source;
    }

    public TResult Max<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository5MethodsCache.Max);
        var result = Source.Provider.QueryFirstOrDefault<TResult>(callExpr);
        return result;
    }

    public TResult Min<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository5MethodsCache.Min);
        var result = Source.Provider.QueryFirstOrDefault<TResult>(callExpr);
        return result;
    }
    public TResult Average<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository5MethodsCache.Average);
        var result = Source.Provider.QueryFirstOrDefault<TResult>(callExpr);
        return result;
    }

    public TResult Sum<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository5MethodsCache.Sum);
        var result = Source.Provider.QueryFirstOrDefault<TResult>(callExpr);
        return result;
    }

    public async Task<TResult> MaxAsync<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository5MethodsCache.Max);
        var result = await Source.Provider.QueryFirstOrDefaultAsync<TResult>(callExpr);
        return result;
    }

    public async Task<TResult> MinAsync<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository5MethodsCache.Min);
        var result = await Source.Provider.QueryFirstOrDefaultAsync<TResult>(callExpr);
        return result;
    }
    public async Task<TResult> AverageAsync<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository5MethodsCache.Average);
        var result = await Source.Provider.QueryFirstOrDefaultAsync<TResult>(callExpr);
        return result;
    }

    public async Task<TResult> SumAsync<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository5MethodsCache.Sum);
        var result = await Source.Provider.QueryFirstOrDefaultAsync<TResult>(callExpr);
        return result;
    }

    private MethodCallExpression GetMaxOrMinOrSumOrAverageMethodCallExpression<TResult>(
        Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector,
        MethodInfo methodInfo
    )
    {
        methodInfo = methodInfo.MakeGenericMethod(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(TResult));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Source.Expression,
            Expression.Quote(selector)
        );
        return callExpr;
    }


    public IJoinGroupRepository<T1, T2, T3, T4, T5, TKey> GroupBy<TKey>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TKey>> selector)
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));
        var methodInfo = JoinRepository5MethodsCache.GroupBy.MakeGenericMethod(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(TKey));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Source.Expression,
            Expression.Quote(selector)
        );
        var r = Source.Provider.CreateQuery<ILambdaRepository<JoinCondition<T1, T2, T3, T4, T5>>>(callExpr);
        var result = new JoinGroupRepository<T1, T2, T3, T4, T5, TKey>(r);
        return result;
    }

    public IJoinRepository<T1, T2, T3, T4, T5> Where(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, bool>> predicate)
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));
        var methodInfo = JoinRepository5MethodsCache.Where.MakeGenericMethod(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Source.Expression,
            Expression.Quote(predicate)
        );

        var r = Source.Provider.CreateQuery<ILambdaRepository<JoinCondition<T1, T2, T3, T4, T5>>>(callExpr);
        var result = new JoinRepository<T1, T2, T3, T4, T5>(r);
        return result;
    }

    public IJoinRepository<T1, T2, T3, T4, T5> WhereIf(bool condition, Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, bool>> predicate)
    {
        if (condition)
        {
            return Where(predicate);
        }

        return this;
    }
    public IJoinOrderRepository<T1, T2, T3, T4, T5> OrderBy<TKey>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TKey>> keySelector)
    {
        var methodInfo = JoinRepository5MethodsCache.OrderBy.MakeGenericMethod(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }

    public IJoinOrderRepository<T1, T2, T3, T4, T5> OrderByDescending<TKey>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TKey>> keySelector)
    {
        var methodInfo = JoinRepository5MethodsCache.OrderByDescending.MakeGenericMethod(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }
    
    public IJoinRepository<T1, T2, T3, T4, T5, T6> LeftJoin<T6>(ILambdaRepository<T6> second, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, bool>> on)
    {
        var methodInfo = JoinRepository6MethodsCache.LeftJoin.MakeGenericMethod(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return InternalJoin(second, on, methodInfo);
    }

    public IJoinRepository<T1, T2, T3, T4, T5, T6> RightJoin<T6>(ILambdaRepository<T6> second, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, bool>> on)
    {
        var methodInfo = JoinRepository6MethodsCache.RightJoin.MakeGenericMethod(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return InternalJoin(second, on, methodInfo);
    }

    public IJoinRepository<T1, T2, T3, T4, T5, T6> InnerJoin<T6>(ILambdaRepository<T6> second, Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, bool>> on)
    {
        var methodInfo = JoinRepository6MethodsCache.InnerJoin.MakeGenericMethod(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return InternalJoin(second, on, methodInfo);
    }

    public ILambdaRepository<TResult> Select<TResult>(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TResult>> selector)
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));

        var methodInfo = JoinRepository5MethodsCache.Select.MakeGenericMethod(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(TResult));
        var callExpr = Expression.Call(
       null,
       methodInfo,
       Source.Expression,
            Expression.Quote(selector)
        );

        var r = Source.Provider.CreateQuery<ILambdaRepository<TResult>>(callExpr);
        return r;
    }
    private MethodCallExpression GetCountMethodCallExpression(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, bool>> predicate)
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));

        var methodInfo = JoinRepository5MethodsCache.Count.MakeGenericMethod(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Source.Expression,
            Expression.Quote(predicate)
        );
        return callExpr;
    }
    public int Count(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, bool>> predicate)
    {
        var callExpr = GetCountMethodCallExpression(predicate);
        var result = Source.Provider.QueryFirstOrDefault<int>(callExpr);

        return result;
    }

    public async Task<int> CountAsync(Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, bool>> predicate)
    {
        var callExpr = GetCountMethodCallExpression(predicate);
        var result = await Source.Provider.QueryFirstOrDefaultAsync<int>(callExpr);

        return result;
    }

    protected IJoinOrderRepository<T1, T2, T3, T4, T5> InternalOrderBy<TKey>(
        Expression<Func<JoinCondition<T1, T2, T3, T4, T5>, TKey>> keySelector,
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

        var r = Source.Provider.CreateQuery<ILambdaRepository<JoinCondition<T1, T2, T3, T4, T5>>>(callExpr);
        var result = new JoinOrderRepository<T1, T2, T3, T4, T5>(r);
        return result;
    }

    protected IJoinRepository<T1, T2, T3, T4, T5, T6> InternalJoin<T6>(
        ILambdaRepository<T6> joinTable,
        Expression<Func<JoinCondition<T1, T2, T3, T4, T5, T6>, bool>> on,
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
            Expression.Quote(on)
        );
        var r = Source.Provider.CreateQuery<ILambdaRepository<JoinCondition<T1, T2, T3, T4, T5, T6>>>(callExpr);
        var result = new JoinRepository<T1, T2, T3, T4, T5, T6>(r);
        return result;
    }

}

public class JoinGroupRepository<T1, T2, T3, T4, T5, TKey> : IJoinGroupRepository<T1, T2, T3, T4, T5, TKey>
{
    public ILambdaRepository<JoinCondition<T1, T2, T3, T4, T5>> Source { get; }

    public JoinGroupRepository(ILambdaRepository<JoinCondition<T1, T2, T3, T4, T5>> source)
    {
        Source = source;
    }
    public ILambdaRepository<TResult> Select<TResult>(Expression<Func<IGrouping<TKey, JoinCondition<T1, T2, T3, T4, T5>>, TResult>> selector)
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));

        var methodInfo = JoinRepository5MethodsCache.GroupBySelect.MakeGenericMethod(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(TKey), typeof(TResult));
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
