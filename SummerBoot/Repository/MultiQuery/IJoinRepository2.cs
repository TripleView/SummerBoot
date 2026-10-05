using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SummerBoot.Repository.MultiQuery;

public interface IExecuteProvider<T1, T2> : IBaseLambdaRepository
{
    int Count(Expression<Func<JoinCondition<T1, T2>, bool>> predicate);

    Task<int> CountAsync(Expression<Func<JoinCondition<T1, T2>, bool>> predicate);

    TResult Max<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector);

    TResult Min<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector);
    TResult Average<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector);

    TResult Sum<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector);


    Task<TResult> MaxAsync<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector);

    Task<TResult> MinAsync<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector);
    Task<TResult> AverageAsync<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector);

    Task<TResult> SumAsync<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector);
}


public interface ISelectProvider<T1, T2> : IBaseLambdaRepository
{
    ISelectStage<TResult> Select<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector);
}

public interface IWhereProvider<T1, T2> : IBaseLambdaRepository
{
    IWhereStage<T1, T2> Where(Expression<Func<JoinCondition<T1, T2>, bool>> predicate);
    IWhereStage<T1, T2> WhereIf(bool condition, Expression<Func<JoinCondition<T1, T2>, bool>> predicate);
}

public interface IWhereStage<T1, T2> : IWhereProvider<T1, T2>, IOrderByProvider<T1, T2>, ISelectProvider<T1, T2>, IGroupByProvider<T1, T2>, IExecuteProvider<T1, T2>
{

}

public interface IOrderByProvider<T1, T2> : IBaseLambdaRepository
{
    IOrderByStage<T1, T2> OrderBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector);
    IOrderByStage<T1, T2> OrderByDescending<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector);
}

public interface IOrderByStage<T1, T2> : IThenByProvider<T1, T2>, ISelectProvider<T1, T2>, IExecuteProvider<T1, T2>
{

}

public interface IThenByProvider<T1, T2> : IBaseLambdaRepository
{
    IThenByStage<T1, T2> ThenBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector);

    IThenByStage<T1, T2> ThenByDescending<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector);
}

public interface IThenByStage<T1, T2> : ISelectProvider<T1, T2>, IThenByProvider<T1, T2>, IExecuteProvider<T1, T2>
{

}

public interface IHavingProvider<T1, T2, TKey>
{
    IHavingStage<T1, T2, TKey> Having(Expression<Func<IGrouping<TKey, JoinCondition<T1, T2>>, bool>> predicate);
}

public interface IHavingStage<T1, T2, TKey> : IGroupBySelectProvider<T1, T2, TKey>
{

}


public interface IGroupByProvider<T1, T2>
{
    IGroupByStage<T1, T2, TKey> GroupBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector);
}


public interface IGroupByStage<T1, T2, TKey> : IGroupBySelectProvider<T1, T2, TKey>, IHavingProvider<T1, T2, TKey>
{

}

public interface IGroupBySelectProvider<T1, T2, TKey> : IBaseLambdaRepository
{
    ISelectStage<TResult> Select<TResult>(Expression<Func<IGrouping<TKey, JoinCondition<T1, T2>>, TResult>> selector);
}


public interface IJoinRepository<T1, T2> : IBaseLambdaRepository, ISelectProvider<T1, T2>, IExecuteProvider<T1, T2>, IOrderByStage<T1, T2>, IThenByStage<T1, T2>, IWhereStage<T1, T2>, IGroupByProvider<T1, T2>
{
    //IJoinRepository<T1, T2, T3> LeftJoin<T3>(ILambdaRepository<T3> table, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on);
    //IJoinRepository<T1, T2, T3> RightJoin<T3>(ILambdaRepository<T3> table, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on);
    //IJoinRepository<T1, T2, T3> InnerJoin<T3>(ILambdaRepository<T3> table, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on);
}


public class JoinRepository<T1, T2> : IJoinRepository<T1, T2>
{
    public ISelectStage<TResult> Select<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        var methodInfo = JoinRepository2MethodsCache.Select.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TResult));
        var callExpr = Expression.Call(
            null,
            methodInfo,
           Expression,
            Expression.Quote(selector)
        );
        return new LambdaRepository<TResult>(callExpr, Provider);
    }

    public Expression Expression { get; }
    public Type ElementType { get; }
    public IRepositoryProvider Provider { get; }

    public JoinRepository(Expression expression, IRepositoryProvider provider)
    {
        Expression = expression;
        Provider = provider;
    }


    private MethodCallExpression GetCountMethodCallExpression(Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
    {
        var methodInfo = JoinRepository2MethodsCache.Count.MakeGenericMethod(typeof(T1), typeof(T2));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Expression,
            Expression.Quote(predicate)
        );
        return callExpr;
    }

    public int Count(Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
    {

        var callExpr = GetCountMethodCallExpression(predicate);
        var result = Provider.QueryFirstOrDefault<int>(callExpr);
        return result;
    }

    public async Task<int> CountAsync(Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
    {
        var callExpr = GetCountMethodCallExpression(predicate);
        var result = await Provider.QueryFirstOrDefaultAsync<int>(callExpr);
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
            Expression,
            Expression.Quote(selector)
        );
        return callExpr;
    }


    public TResult Max<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository2MethodsCache.Max);
        var result = Provider.QueryFirstOrDefault<TResult>(callExpr);
        return result;
    }

    public TResult Min<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository2MethodsCache.Min);
        var result = Provider.QueryFirstOrDefault<TResult>(callExpr);
        return result;
    }

    public TResult Average<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository2MethodsCache.Average);
        var result = Provider.QueryFirstOrDefault<TResult>(callExpr);
        return result;
    }

    public TResult Sum<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository2MethodsCache.Sum);
        var result = Provider.QueryFirstOrDefault<TResult>(callExpr);
        return result;
    }

    public async Task<TResult> MaxAsync<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository2MethodsCache.Max);
        var result = await Provider.QueryFirstOrDefaultAsync<TResult>(callExpr);
        return result;
    }

    public async Task<TResult> MinAsync<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository2MethodsCache.Min);
        var result = await Provider.QueryFirstOrDefaultAsync<TResult>(callExpr);
        return result;
    }

    public async Task<TResult> AverageAsync<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository2MethodsCache.Average);
        var result = await Provider.QueryFirstOrDefaultAsync<TResult>(callExpr);
        return result;
    }

    public async Task<TResult> SumAsync<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            JoinRepository2MethodsCache.Sum);
        var result = await Provider.QueryFirstOrDefaultAsync<TResult>(callExpr);
        return result;
    }
    protected JoinRepository<T1, T2> InternalOrderBy<TKey>(
        Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector,
        MethodInfo methodInfo
    )
    {
        methodInfo = methodInfo.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Expression,
            Expression.Quote(keySelector)
        );

        var result = new JoinRepository<T1, T2>(callExpr, Provider);
        return result;
    }
    public IThenByStage<T1, T2> ThenBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        var result = InternalOrderBy(keySelector,
            JoinRepository2MethodsCache.ThenBy);
        return result;
    }

    public IThenByStage<T1, T2> ThenByDescending<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        var result = InternalOrderBy(keySelector,
            JoinRepository2MethodsCache.ThenBy);
        return result;
    }

    public IWhereStage<T1, T2> Where(Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
    {
        var methodInfo = JoinRepository2MethodsCache.Where.MakeGenericMethod(typeof(T1), typeof(T2));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Expression,
            Expression.Quote(predicate)
        );

        var result = new JoinRepository<T1, T2>(callExpr, Provider);
        return result;
    }

    public IWhereStage<T1, T2> WhereIf(bool condition, Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
    {
        if (condition)
        {
            return Where(predicate);
        }

        return this;
    }

    public IOrderByStage<T1, T2> OrderBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        var result = InternalOrderBy(keySelector,
            JoinRepository2MethodsCache.OrderBy);
        return result;
    }

    public IOrderByStage<T1, T2> OrderByDescending<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        var result = InternalOrderBy(keySelector,
            JoinRepository2MethodsCache.OrderByDescending);
        return result;
    }

    public IGroupByStage<T1, T2, TKey> GroupBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
    {
        var methodInfo = JoinRepository2MethodsCache.GroupBy.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Expression,
            Expression.Quote(keySelector)
        );
        return new GroupRepository<T1, T2, TKey>(callExpr, Provider);
    }
}



public class GroupRepository<T1, T2, TKey> : IGroupByStage<T1, T2, TKey>, IBaseLambdaRepository, IHavingStage<T1, T2, TKey>
{
    public GroupRepository(Expression expression, IRepositoryProvider provider)
    {
        this.Expression = expression;
        this.Provider = provider;
    }
    public IRepositoryProvider Provider { get; protected set; }

    public Type ElementType => typeof(T1);

    public ISelectStage<TResult> Select<TResult>(Expression<Func<IGrouping<TKey, JoinCondition<T1, T2>>, TResult>> selector)
    {
        var methodInfo = JoinRepository2MethodsCache.GroupBySelect.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey), typeof(TResult));
        var callExpr = Expression.Call(
            null,
            methodInfo,
           Expression,
            Expression.Quote(selector)
        );

        return new LambdaRepository<TResult>(callExpr, Provider);
    }

    public Expression Expression { get; protected set; }

    public IHavingStage<T1, T2, TKey> Having(Expression<Func<IGrouping<TKey, JoinCondition<T1, T2>>, bool>> predicate)
    {
        var methodInfo = JoinRepository2MethodsCache.Having.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Expression,
            Expression.Quote(predicate)
        );

        return new GroupRepository<T1, T2, TKey>(callExpr, Provider);
    }
}


