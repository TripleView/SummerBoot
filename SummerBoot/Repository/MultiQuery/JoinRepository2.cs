//using System.Collections.Generic;
//using System.Linq.Expressions;
//using System.Linq;
//using System.Reflection;
//using System;
//using System.Threading.Tasks;

//namespace SummerBoot.Repository.MultiQuery;


//public class HavingRepository<T1, T2, TKey> : IHavingRepository<T1, T2, TKey>
//{
//    public IBaseLambdaRepository Source { get; }

//    public Expression Expression => Source.Expression;

//    public Type ElementType => Source.ElementType;

//    public IRepositoryProvider Provider => Source.Provider;

//    public HavingRepository(IBaseLambdaRepository source)
//    {
//        Source = source;
//    }
//    public IGroupBySelectRepository<T1, T2, TKey> Having<TResult>(Expression<Func<IGrouping<TKey, JoinCondition<T1, T2>>, TResult>> predicate)
//    {
//        if (Source == null) throw new ArgumentNullException(nameof(Source));

//        var methodInfo = JoinRepository2MethodsCache.Having.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey), typeof(TResult));
//        var callExpr = Expression.Call(
//            null,
//            methodInfo,
//            Source.Expression,
//            Expression.Quote(predicate)
//        );

//        var r = Source.Provider.CreateQuery<IGroupBySelectRepository<T1, T2, TKey>>(callExpr);
//        return r;
//    }
//}


//public class SelectRepository<T1, T2> : ISelectRepository<T1, T2>
//{
//    public IBaseLambdaRepository Source { get; }

//    public Expression Expression => Source.Expression;

//    public Type ElementType => Source.ElementType;

//    public IRepositoryProvider Provider => Source.Provider;

//    public SelectRepository(IBaseLambdaRepository source)
//    {
//        Source = source;
//    }
//    public ILambdaRepository<TResult> Select<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
//    {
//        if (Source == null) throw new ArgumentNullException(nameof(Source));

//        var methodInfo = JoinRepository2MethodsCache.Select.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TResult));
//        var callExpr = Expression.Call(
//            null,
//            methodInfo,
//            Source.Expression,
//            Expression.Quote(selector)
//        );

//        var r = Source.Provider.CreateQuery<ILambdaRepository<TResult>>(callExpr);
//        return r;
//    }
//}

//public class WhereRepository<T1, T2> : IWhereRepository<T1, T2>
//{
//    public Expression Expression => Source.Expression;

//    public Type ElementType => Source.ElementType;

//    public IRepositoryProvider Provider => Source.Provider;
//    public IBaseLambdaRepository Source { get; }

//    public WhereRepository(IBaseLambdaRepository source)
//    {
//        Source = source;
//    }
//    public ISelectOrOrderByOrGroupBy<T1, T2> Where(Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
//    {
//        if (Source == null) throw new ArgumentNullException(nameof(Source));
//        var methodInfo = JoinRepository2MethodsCache.Where.MakeGenericMethod(typeof(T1), typeof(T2));
//        var callExpr = Expression.Call(
//            null,
//            methodInfo,
//            Source.Expression,
//            Expression.Quote(predicate)
//        );

//        var r = Source.Provider.CreateQuery<ISelectOrOrderByOrGroupBy<T1, T2>>(callExpr);
//        return r;
//    }

//    public ISelectOrOrderByOrGroupBy<T1, T2> WhereIf(bool condition, Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
//    {
//        if (condition)
//        {
//            return Where(predicate);
//        }

//        return new SelectOrOrderByOrGroupBy<T1, T2>(Source.Expression);
//    }
//}


//public class OrderByRepository<T1, T2> : IOrderByRepository<T1, T2>
//{
//    public Expression Expression => Source.Expression;

//    public Type ElementType => Source.ElementType;

//    public IRepositoryProvider Provider => Source.Provider;
//    public IBaseLambdaRepository Source { get; }

//    public OrderByRepository(IBaseLambdaRepository source)
//    {
//        Source = source;
//    }
//    public IThenByRepository<T1, T2> OrderBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
//    {
//        var methodInfo = JoinRepository2MethodsCache.OrderBy.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey));
//        var result = InternalOrderBy(keySelector, methodInfo);
//        return result;
//    }

//    public IThenByRepository<T1, T2> OrderByDescending<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
//    {
//        var methodInfo = JoinRepository2MethodsCache.OrderByDescending.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey));
//        var result = InternalOrderBy(keySelector, methodInfo);
//        return result;
//    }

//    protected IThenByRepository<T1, T2> InternalOrderBy<TKey>(
//        Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector,
//        MethodInfo methodInfo
//    )
//    {
//        if (Source == null) throw new ArgumentNullException(nameof(Source));

//        var callExpr = Expression.Call(
//            null,
//            methodInfo,
//            Source.Expression,
//            Expression.Quote(keySelector)
//        );

//        var r = Source.Provider.CreateQuery<ILambdaRepository<JoinCondition<T1, T2>>>(callExpr);
//        var result = new JoinOrderRepository<T1, T2>(r);
//        return result;
//    }
//}

//public class ThenByRepository<T1, T2> : IThenByRepository<T1, T2>
//{
//    public Expression Expression => Source.Expression;

//    public Type ElementType => Source.ElementType;

//    public IRepositoryProvider Provider => Source.Provider;
//    public ILambdaRepository<JoinCondition<T1, T2>> Source { get; }

//    public ThenByRepository(ILambdaRepository<JoinCondition<T1, T2>> source)
//    {
//        Source = source;
//    }
//    public IThenByOrSelect<T1, T2> ThenBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
//    {
//        var methodInfo = JoinRepository2MethodsCache.ThenBy.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey));
//        var result = InternalOrderBy(keySelector, methodInfo);
//        return result;
//    }

//    public IThenByOrSelect<T1, T2> ThenByDescending<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
//    {
//        var methodInfo = JoinRepository2MethodsCache.ThenByDescending.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey));
//        var result = InternalOrderBy(keySelector, methodInfo);
//        return result;
//    }

//    protected IThenByOrSelect<T1, T2> InternalOrderBy<TKey>(
//        Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector,
//        MethodInfo methodInfo
//    )
//    {
//        if (Source == null) throw new ArgumentNullException(nameof(Source));

//        var callExpr = Expression.Call(
//            null,
//            methodInfo,
//            Source.Expression,
//            Expression.Quote(keySelector)
//        );

//        var r = Source.Provider.CreateQuery<ILambdaRepository<JoinCondition<T1, T2>>>(callExpr);
//        var result = new JoinOrderRepository<T1, T2>(r);
//        return result;
//    }
//}

//public class GroupByRepository<T1, T2> : IGroupByRepository<T1, T2>
//{
//    public Expression Expression => Source.Expression;

//    public Type ElementType => Source.ElementType;

//    public IRepositoryProvider Provider => Source.Provider;
//    public ILambdaRepository<JoinCondition<T1, T2>> Source { get; }

//    public GroupByRepository(ILambdaRepository<JoinCondition<T1, T2>> source)
//    {
//        Source = source;
//    }
//    public IGroupBySelectOrHavingRepository<T1, T2, TKey> GroupBy<TKey>(
//        Expression<Func<JoinCondition<T1, T2>, TKey>> selector)
//    {
//        if (Source == null) throw new ArgumentNullException(nameof(Source));
//        var methodInfo = JoinRepository2MethodsCache.GroupBy.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey));
//        var callExpr = Expression.Call(
//            null,
//            methodInfo,
//            Source.Expression,
//            Expression.Quote(selector)
//        );
//        var r = Source.Provider.CreateQuery<ILambdaRepository<JoinCondition<T1, T2>>>(callExpr);
//        var result = new JoinGroupRepository<T1, T2, TKey>(r);
//        return result;
//    }
//}

//public class GroupBySelectRepository<T1, T2, TKey> : IGroupBySelectRepository<T1, T2, TKey>
//{
//    public Expression Expression => Source.Expression;

//    public Type ElementType => Source.ElementType;

//    public IRepositoryProvider Provider => Source.Provider;
//    public IGroupBySelectOrHavingRepository<T1, T2, TKey> Source { get; }

//    public GroupBySelectRepository(IGroupBySelectOrHavingRepository<T1, T2, TKey> source)
//    {
//        Source = source;
//    }

//    public ILambdaRepository<TResult> Select<TResult>(Expression<Func<IGrouping<TKey, JoinCondition<T1, T2>>, TResult>> selector)
//    {
//        if (Source == null) throw new ArgumentNullException(nameof(Source));

//        var methodInfo = JoinRepository2MethodsCache.GroupBySelect.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TKey), typeof(TResult));
//        var callExpr = Expression.Call(
//            null,
//            methodInfo,
//            Source.Expression,
//            Expression.Quote(selector)
//        );

//        var r = Source.Provider.CreateQuery<ILambdaRepository<TResult>>(callExpr);
//        return r;
//    }
//}

//public class ThenByOrSelect<T1, T2> : IThenByOrSelect<T1, T2>
//{
//    public Type ElementType => Source.ElementType;

//    public IRepositoryProvider Provider => Source.Provider;
//    public OrderByRepository<T1, T2> Source { get; }

//    public Expression Expression => Source.Expression;

//    private readonly ISelectRepository<T1, T2> selectRepository;

//    private readonly IThenByRepository<T1, T2> thenByRepository;
//    public ThenByOrSelect(OrderByRepository<T1, T2> source)
//    {
//        Source = source;
//        selectRepository = new SelectRepository<T1, T2>(source.Source);
//        thenByRepository = new ThenByRepository<T1, T2>(source.Source);
//    }
//    public ILambdaRepository<TResult> Select<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
//    {
//        return selectRepository.Select(selector);
//    }

//    public IThenByOrSelect<T1, T2> ThenBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
//    {
//        return thenByRepository.ThenBy(keySelector);
//    }

//    public IThenByOrSelect<T1, T2> ThenByDescending<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
//    {
//        return thenByRepository.ThenByDescending(keySelector);
//    }
//}

//public class SelectOrWhereOrOrderByOrGroupBy<T1, T2> : ISelectOrWhereOrOrderByOrGroupBy<T1, T2>
//{
//    public Type ElementType => Source.ElementType;

//    public IRepositoryProvider Provider => Source.Provider;
//    public OrderByRepository<T1, T2> Source { get; }

//    public Expression Expression => Source.Expression;

//    private readonly ISelectRepository<T1, T2> selectRepository;

//    private readonly IOrderByRepository<T1, T2> orderByRepository;

//    private readonly IWhereRepository<T1, T2> whereRepository;

//    private readonly IGroupByRepository<T1, T2> groupByRepository;
//    public SelectOrWhereOrOrderByOrGroupBy(OrderByRepository<T1, T2> source)
//    {
//        Source = source;
//        selectRepository = new SelectRepository<T1, T2>(source.Source);
//        orderByRepository = new OrderByRepository<T1, T2>(source.Source);
//        whereRepository = new WhereRepository<T1, T2>(source.Source);
//        groupByRepository = new GroupByRepository<T1, T2>(source.Source);
//    }
//    public ILambdaRepository<TResult> Select<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
//    {
//        return selectRepository.Select(selector);
//    }

//    public ISelectOrOrderByOrGroupBy<T1, T2> Where(Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
//    {
//        return whereRepository.Where(predicate);
//    }

//    public ISelectOrOrderByOrGroupBy<T1, T2> WhereIf(bool condition, Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
//    {
//        return whereRepository.WhereIf(condition,predicate);
//    }

//    public IThenByRepository<T1, T2> OrderBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
//    {
//        return orderByRepository.OrderBy(keySelector);
//    }

//    public IThenByRepository<T1, T2> OrderByDescending<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
//    {
//        return orderByRepository.OrderBy(keySelector);
//    }

//    public IGroupBySelectOrHavingRepository<T1, T2, TKey> GroupBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> selector)
//    {
//        return groupByRepository.GroupBy(selector);
//    }
//}


//public class SelectOrOrderByOrGroupBy<T1, T2> : ISelectOrOrderByOrGroupBy<T1, T2>
//{
//    public Type ElementType => Source.ElementType;

//    public IRepositoryProvider Provider => Source.Provider;
//    public OrderByRepository<T1, T2> Source { get; }

//    public Expression Expression => Source.Expression;

//    private readonly ISelectRepository<T1, T2> selectRepository;

//    private readonly IOrderByRepository<T1, T2> orderByRepository;

//    private readonly IGroupByRepository<T1, T2> groupByRepository;
//    public SelectOrOrderByOrGroupBy(OrderByRepository<T1, T2> source)
//    {
//        Source = source;
//        selectRepository = new SelectRepository<T1, T2>(source.Source);
//        orderByRepository = new OrderByRepository<T1, T2>(source.Source);
//        groupByRepository = new GroupByRepository<T1, T2>(source.Source);
//    }
//    public ILambdaRepository<TResult> Select<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
//    {
//        return selectRepository.Select(selector);
//    }

//    public IThenByRepository<T1, T2> OrderBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
//    {
//        return orderByRepository.OrderBy(keySelector);
//    }

//    public IThenByRepository<T1, T2> OrderByDescending<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
//    {
//        return orderByRepository.OrderBy(keySelector);
//    }

//    public IGroupBySelectOrHavingRepository<T1, T2, TKey> GroupBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> selector)
//    {
//        return groupByRepository.GroupBy(selector);
//    }
//}

//public class GroupBySelectOrHavingRepository<T1, T2, TKey> : IGroupBySelectOrHavingRepository<T1, T2, TKey>
//{
//    public Type ElementType => Source.ElementType;

//    public IRepositoryProvider Provider => Source.Provider;
//    public GroupBySelectOrHavingRepository<T1, T2, TKey>  Source { get; }

//    public Expression Expression => Source.Expression;

//    private readonly IGroupBySelectRepository<T1, T2, TKey> groupBySelectRepository;

//    private readonly IHavingRepository<T1, T2, TKey> havingRepository;

//    public GroupBySelectOrHavingRepository(GroupBySelectOrHavingRepository<T1, T2, TKey> source)
//    {
//        Source = source;
//        groupBySelectRepository = new GroupBySelectRepository<T1, T2, TKey>(source.Source);
//        havingRepository = new HavingRepository<T1, T2, TKey>(source.Source);
//    }

//    public IGroupBySelectRepository<T1, T2, TKey> Having<TResult>(Expression<Func<IGrouping<TKey, JoinCondition<T1, T2>>, TResult>> predicate)
//    {
//        return havingRepository.Having(predicate);
//    }

//    public ILambdaRepository<TResult> Select<TResult>(Expression<Func<IGrouping<TKey, JoinCondition<T1, T2>>, TResult>> selector)
//    {
//        return groupBySelectRepository.Select(selector);
//    }
//}


//public class JoinRepository<T1, T2> : IJoinRepository<T1, T2>
//{
//    public ILambdaRepository<JoinCondition<T1, T2>> Source { get; }

//    public JoinRepository(ILambdaRepository<JoinCondition<T1, T2>> source)
//    {
//        Source = source;
//    }

//    public TResult Max<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
//    {
//        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
//            JoinRepository2MethodsCache.Max);
//        var result = Source.Provider.QueryFirstOrDefault<TResult>(callExpr);
//        return result;
//    }

//    public TResult Min<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
//    {
//        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
//            JoinRepository2MethodsCache.Min);
//        var result = Source.Provider.QueryFirstOrDefault<TResult>(callExpr);
//        return result;
//    }
//    public TResult Average<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
//    {
//        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
//            JoinRepository2MethodsCache.Average);
//        var result = Source.Provider.QueryFirstOrDefault<TResult>(callExpr);
//        return result;
//    }

//    public TResult Sum<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
//    {
//        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
//            JoinRepository2MethodsCache.Sum);
//        var result = Source.Provider.QueryFirstOrDefault<TResult>(callExpr);
//        return result;
//    }

//    public async Task<TResult> MaxAsync<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
//    {
//        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
//            JoinRepository2MethodsCache.Max);
//        var result = await Source.Provider.QueryFirstOrDefaultAsync<TResult>(callExpr);
//        return result;
//    }

//    public async Task<TResult> MinAsync<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
//    {
//        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
//            JoinRepository2MethodsCache.Min);
//        var result = await Source.Provider.QueryFirstOrDefaultAsync<TResult>(callExpr);
//        return result;
//    }
//    public async Task<TResult> AverageAsync<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
//    {
//        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
//            JoinRepository2MethodsCache.Average);
//        var result = await Source.Provider.QueryFirstOrDefaultAsync<TResult>(callExpr);
//        return result;
//    }

//    public async Task<TResult> SumAsync<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
//    {
//        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
//            JoinRepository2MethodsCache.Sum);
//        var result = await Source.Provider.QueryFirstOrDefaultAsync<TResult>(callExpr);
//        return result;
//    }

//    private MethodCallExpression GetMaxOrMinOrSumOrAverageMethodCallExpression<TResult>(
//        Expression<Func<JoinCondition<T1, T2>, TResult>> selector,
//        MethodInfo methodInfo
//    )
//    {
//        methodInfo = methodInfo.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TResult));
//        var callExpr = Expression.Call(
//            null,
//            methodInfo,
//            Source.Expression,
//            Expression.Quote(selector)
//        );
//        return callExpr;
//    }

//    public IJoinRepository<T1, T2, T3> LeftJoin<T3>(ILambdaRepository<T3> second, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on)
//    {
//        var methodInfo = JoinRepository3MethodsCache.LeftJoin.MakeGenericMethod(typeof(T1), typeof(T2), typeof(T3));
//        return InternalJoin(second, on, methodInfo);
//    }

//    public IJoinRepository<T1, T2, T3> RightJoin<T3>(ILambdaRepository<T3> second, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on)
//    {
//        var methodInfo = JoinRepository3MethodsCache.RightJoin.MakeGenericMethod(typeof(T1), typeof(T2), typeof(T3));
//        return InternalJoin(second, on, methodInfo);
//    }

//    public IJoinRepository<T1, T2, T3> InnerJoin<T3>(ILambdaRepository<T3> second, Expression<Func<JoinCondition<T1, T2, T3>, bool>> on)
//    {
//        var methodInfo = JoinRepository3MethodsCache.InnerJoin.MakeGenericMethod(typeof(T1), typeof(T2), typeof(T3));
//        return InternalJoin(second, on, methodInfo);
//    }

//    public ILambdaRepository<TResult> Select<TResult>(Expression<Func<JoinCondition<T1, T2>, TResult>> selector)
//    {
//        if (Source == null) throw new ArgumentNullException(nameof(Source));

//        var methodInfo = JoinRepository2MethodsCache.Select.MakeGenericMethod(typeof(T1), typeof(T2), typeof(TResult));
//        var callExpr = Expression.Call(
//       null,
//       methodInfo,
//       Source.Expression,
//            Expression.Quote(selector)
//        );

//        var r = Source.Provider.CreateQuery<ILambdaRepository<TResult>>(callExpr);
//        return r;
//    }

//    private MethodCallExpression GetCountMethodCallExpression(Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
//    {
//        if (Source == null) throw new ArgumentNullException(nameof(Source));

//        var methodInfo = JoinRepository2MethodsCache.Count.MakeGenericMethod(typeof(T1), typeof(T2));
//        var callExpr = Expression.Call(
//            null,
//            methodInfo,
//            Source.Expression,
//            Expression.Quote(predicate)
//        );
//        return callExpr;
//    }

//    public int Count(Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
//    {
//        var callExpr = GetCountMethodCallExpression(predicate);
//        var result = Source.Provider.QueryFirstOrDefault<int>(callExpr);

//        return result;
//    }

//    public async Task<int> CountAsync(Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
//    {
//        var callExpr = GetCountMethodCallExpression(predicate);
//        var result = await Source.Provider.QueryFirstOrDefaultAsync<int>(callExpr);

//        return result;
//    }

//    protected IJoinRepository<T1, T2, T3> InternalJoin<T3>(
//        ILambdaRepository<T3> joinTable,
//        Expression<Func<JoinCondition<T1, T2, T3>, bool>> on,
//        MethodInfo methodInfo
//    )
//    {
//        if (Source == null) throw new ArgumentNullException(nameof(Source));
//        if (joinTable == null) throw new ArgumentNullException(nameof(joinTable));
//        if (on == null) throw new ArgumentNullException(nameof(on));

//        var callExpr = Expression.Call(
//            null,
//            methodInfo,
//            Source.Expression,
//            joinTable.Expression,
//            Expression.Quote(on)
//        );
//        var r = Source.Provider.CreateQuery<ILambdaRepository<JoinCondition<T1, T2, T3>>>(callExpr);
//        var result = new JoinRepository<T1, T2, T3>(r);
//        return result;
//    }

//    public Expression Expression { get; }
//    public Type ElementType { get; }
//    public IRepositoryProvider Provider { get; }
//    public ISelectOrOrderByOrGroupBy<T1, T2> Where(Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
//    {
//        throw new NotImplementedException();
//    }

//    public ISelectOrOrderByOrGroupBy<T1, T2> WhereIf(bool condition, Expression<Func<JoinCondition<T1, T2>, bool>> predicate)
//    {
//        throw new NotImplementedException();
//    }

//    public IThenByRepository<T1, T2> OrderBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
//    {
//        throw new NotImplementedException();
//    }

//    public IThenByRepository<T1, T2> OrderByDescending<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> keySelector)
//    {
//        throw new NotImplementedException();
//    }

//    public IGroupBySelectOrHavingRepository<T1, T2, TKey> GroupBy<TKey>(Expression<Func<JoinCondition<T1, T2>, TKey>> selector)
//    {
//        throw new NotImplementedException();
//    }
//}