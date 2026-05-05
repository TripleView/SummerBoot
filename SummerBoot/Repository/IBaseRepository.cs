using SqlParser.Net;
using SummerBoot.Repository.MultiQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SummerBoot.Repository;

public interface ILambdaRepository<T>
{
    Expression Expression { get; }
    Type ElementType => typeof(T);
    IRepositoryProvider Provider { get; }
    ILambdaRepository<T> Where(Expression<Func<T, bool>> predicate);
    ILambdaRepository<T> WhereIf(bool condition, Expression<Func<T, bool>> predicate);
    IOrderLambdaRepository<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);
    IOrderLambdaRepository<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
    IGroupLambdaRepository<T, TKey> GroupBy<TKey>(Expression<Func<T, TKey>> keySelector);

    ILambdaRepository<TResult> Select<TResult>(Expression<Func<T, TResult>> selector);

    List<T> ToList();

    Task<List<T>> ToListAsync();

    IPageLambdaRepository<T> Take(int count);
    IPageLambdaRepository<T> Skip(int count);

    int Count();
    Task<int> CountAsync();

    int Count(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);

    T First();
    Task<T> FirstAsync();

    T First(Expression<Func<T, bool>> predicate);
    Task<T> FirstAsync(Expression<Func<T, bool>> predicate);

    T FirstOrDefault();
    Task<T> FirstOrDefaultAsync();

    T FirstOrDefault(Expression<Func<T, bool>> predicate);
    Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// 生成经过分页的结果
    /// </summary>
    /// <returns></returns>
    Page<T> ToPage(IPageable pageable);

    /// <summary>
    /// 生成经过分页的结果
    /// </summary>
    /// <returns></returns>
    Task<Page<T>> ToPageAsync(IPageable pageable);

    TResult Max<TResult>(Expression<Func<T, TResult>> selector);

    Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> selector);
    TResult Min<TResult>(Expression<Func<T, TResult>> selector);

    Task<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> selector);
    TResult Sum<TResult>(Expression<Func<T, TResult>> selector);

    Task<TResult> SumAsync<TResult>(Expression<Func<T, TResult>> selector);
    TResult Average<TResult>(Expression<Func<T, TResult>> selector);

    Task<TResult> AverageAsync<TResult>(Expression<Func<T, TResult>> selector);

    int ExecuteUpdate();
    Task<int> ExecuteUpdateAsync();
}

public interface IGroupLambdaRepository<T, TKey>
{
    IBaseRepository<TResult> Select<TResult>(Expression<Func<IGrouping<TKey, TResult>>> selector);
}

public class GroupLambdaRepository<T, TKey> : IGroupLambdaRepository<T, TKey>
{
    public IBaseRepository<T> Source { get; }

    public GroupLambdaRepository(IBaseRepository<T> source)
    {
        Source = source;
    }
    public IBaseRepository<TResult> Select<TResult>(Expression<Func<IGrouping<TKey, TResult>>> selector)
    {
        if (Source == null) throw new ArgumentNullException(nameof(Source));

        var methodInfo = RepositoryMethodsCache.GroupBySelect.MakeGenericMethod(typeof(T), typeof(TKey), typeof(TResult));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Source.Expression,
            Expression.Quote(selector)
        );

        var r = Source.Provider.CreateQuery<IBaseRepository<TResult>>(callExpr);
        return r;
    }
}



public interface IOrderLambdaRepository<T> : ILambdaRepository<T>
{
    IOrderLambdaRepository<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector);
    IOrderLambdaRepository<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
}

public class OrderLambdaRepository<T> : IOrderLambdaRepository<T>
{
    public IRepositoryProvider Provider { get; protected set; }

    public Type ElementType => typeof(T);

    public Expression Expression { get; protected set; }

    public void SetExpressionAndProvider(Expression expression, IRepositoryProvider provider)
    {
        this.Expression = expression;
        this.Provider = provider;
    }

    public ILambdaRepository<T> Where(Expression<Func<T, bool>> predicate)
    {
        var methodInfo = RepositoryMethodsCache.Where.MakeGenericMethod(typeof(T));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            this.Expression,
            Expression.Quote(predicate)
        );

        var result = Provider.CreateQuery<ILambdaRepository<T>>(callExpr);
        return result;
    }

    public ILambdaRepository<T> WhereIf(bool condition, Expression<Func<T, bool>> predicate)
    {
        if (condition)
        {
            return Where(predicate);
        }

        return this;
    }

    private IOrderLambdaRepository<T> InternalOrderBy<TKey>(
        Expression<Func<T, TKey>> keySelector,
        MethodInfo methodInfo
    )
    {
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Expression,
            Expression.Quote(keySelector)
        );

        var result = Provider.CreateQuery<IOrderLambdaRepository<T>>(callExpr);
        return result;
    }

    public IOrderLambdaRepository<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        var methodInfo = RepositoryMethodsCache.OrderBy.MakeGenericMethod(typeof(T), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }

    public IOrderLambdaRepository<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        var methodInfo = RepositoryMethodsCache.OrderByDescending.MakeGenericMethod(typeof(T), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }

    public IOrderLambdaRepository<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        var methodInfo = RepositoryMethodsCache.ThenBy.MakeGenericMethod(typeof(T), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }

    public IOrderLambdaRepository<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        var methodInfo = RepositoryMethodsCache.ThenByDescending.MakeGenericMethod(typeof(T), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }

    public IGroupLambdaRepository<T, TKey> GroupBy<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        var methodInfo = RepositoryMethodsCache.GroupBy.MakeGenericMethod(typeof(T), typeof(TKey));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Expression,
            Expression.Quote(keySelector)
        );
        var r = Provider.CreateQuery<IBaseRepository<T>>(callExpr);
        var result = new GroupLambdaRepository<T, TKey>(r);
        return result;
    }

    public ILambdaRepository<TResult> Select<TResult>(Expression<Func<T, TResult>> selector)
    {
        var methodInfo = RepositoryMethodsCache.Select.MakeGenericMethod(typeof(T), typeof(TResult));
        var callExpr = Expression.Call(
            null,
            methodInfo,
           Expression,
            Expression.Quote(selector)
        );

        var r = Provider.CreateQuery<ILambdaRepository<TResult>>(callExpr);
        return r;
    }

    public List<T> ToList()
    {
        var result = Provider.QueryList<T>(Expression);
        return result;
    }

    public async Task<List<T>> ToListAsync()
    {
        var result = Provider.QueryList<T>(Expression);
        return result;
    }
    private IPageLambdaRepository<T> InternalTakeSkip(
        int count,
        MethodInfo methodInfo
    )
    {
        methodInfo = methodInfo.MakeGenericMethod(typeof(T));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Expression,
            Expression.Constant(count)
        );

        var result = Provider.CreateQuery<IPageLambdaRepository<T>>(callExpr);

        return result;
    }


    public IPageLambdaRepository<T> Take(int count)
    {
        var result = InternalTakeSkip(count, RepositoryMethodsCache.Take);
        return result;
    }

    public IPageLambdaRepository<T> Skip(int count)
    {
        var result = InternalTakeSkip(count, RepositoryMethodsCache.Skip);
        return result;
    }


    private MethodCallExpression GetCountMethodCallExpression()
    {
        var methodInfo = RepositoryMethodsCache.Count.MakeGenericMethod(typeof(T));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Expression
        );
        return callExpr;
    }

    public int Count()
    {
        var callExpr = GetCountMethodCallExpression();
        var result = Provider.QueryFirstOrDefault<int>(callExpr);

        return result;
    }

    public async Task<int> CountAsync()
    {
        var callExpr = GetCountMethodCallExpression();
        var result = await Provider.QueryFirstOrDefaultAsync<int>(callExpr);

        return result;
    }

    public int Count(Expression<Func<T, bool>> predicate)
    {
        var result = this.Where(predicate).Count();
        return result;
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        var result = await this.Where(predicate).CountAsync();
        return result;
    }

    private MethodCallExpression GetFirstMethodCallExpression()
    {
        var methodInfo = RepositoryMethodsCache.First.MakeGenericMethod(typeof(T));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Expression
        );
        return callExpr;
    }

    public T First()
    {
        var callExpr = GetFirstMethodCallExpression();
        var result = Provider.QueryFirstOrDefault<T>(callExpr);

        return result;
    }

    public async Task<T> FirstAsync()
    {
        var callExpr = GetFirstMethodCallExpression();
        var result = await Provider.QueryFirstOrDefaultAsync<T>(callExpr);

        return result;
    }

    public T First(Expression<Func<T, bool>> predicate)
    {
        var result = this.Where(predicate).First();
        return result;
    }

    public async Task<T> FirstAsync(Expression<Func<T, bool>> predicate)
    {
        var result = await this.Where(predicate).FirstAsync();
        return result;
    }

    public T FirstOrDefault()
    {
        return this.First();
    }

    public async Task<T> FirstOrDefaultAsync()
    {
        return await this.FirstAsync();
    }

    public T FirstOrDefault(Expression<Func<T, bool>> predicate)
    {
        var result = this.Where(predicate).FirstOrDefault();
        return result;
    }

    public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        var result = await this.Where(predicate).FirstOrDefaultAsync();
        return result;
    }

    private MethodCallExpression GetToPageMethodCallExpression()
    {
        var methodInfo = RepositoryMethodsCache.ToPageNoPageParameter.MakeGenericMethod(typeof(T));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Expression
        );
        return callExpr;
    }

    public Page<T> ToPage()
    {
        var callExpr = GetToPageMethodCallExpression();
        var result =  Provider.QueryPage<T>(callExpr);

        return result;
    }

    public async Task<Page<T>> ToPageAsync()
    {
        var callExpr = GetToPageMethodCallExpression();
        var result =await Provider.QueryPageAsync<T>(callExpr);

        return result;
    }
    private MethodCallExpression GetToPageWithParameterMethodCallExpression(IPageable pageable)
    {
        var methodInfo = RepositoryMethodsCache.ToPage.MakeGenericMethod(typeof(T));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Expression,
            System.Linq.Expressions.Expression.Constant(pageable)
        );
        return callExpr;
    }
    public Page<T> ToPage(IPageable pageable)
    {
        var callExpr = GetToPageWithParameterMethodCallExpression(pageable);
        var result = Provider.QueryPage<T>(callExpr);

        return result;
    }

    public async Task<Page<T>> ToPageAsync(IPageable pageable)
    {
        var callExpr = GetToPageWithParameterMethodCallExpression(pageable);
        var result = await Provider.QueryPageAsync<T>(callExpr);

        return result;
    }


    private MethodCallExpression GetMaxOrMinOrSumOrAverageMethodCallExpression<TResult>(
        Expression<Func<T, TResult>> selector,
        MethodInfo methodInfo
    )
    {
        methodInfo = methodInfo.MakeGenericMethod(typeof(T), typeof(TResult));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Expression,
            Expression.Quote(selector)
        );
        return callExpr;
    }

    public TResult Max<TResult>(Expression<Func<T, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            RepositoryMethodsCache.Max);
        var result = Provider.QueryFirstOrDefault<TResult>(callExpr);
        return result;
    }

    public async Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            RepositoryMethodsCache.Max);
        var result = await Provider.QueryFirstOrDefaultAsync<TResult>(callExpr);
        return result;
    }

    public TResult Min<TResult>(Expression<Func<T, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            RepositoryMethodsCache.Min);
        var result = Provider.QueryFirstOrDefault<TResult>(callExpr);
        return result;
    }

    public async Task<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            RepositoryMethodsCache.Min);
        var result = await Provider.QueryFirstOrDefaultAsync<TResult>(callExpr);
        return result;
    }

    public TResult Sum<TResult>(Expression<Func<T, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            RepositoryMethodsCache.Sum);
        var result = Provider.QueryFirstOrDefault<TResult>(callExpr);
        return result;
    }

    public async Task<TResult> SumAsync<TResult>(Expression<Func<T, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            RepositoryMethodsCache.Sum);
        var result = await Provider.QueryFirstOrDefaultAsync<TResult>(callExpr);
        return result;
    }

    public TResult Average<TResult>(Expression<Func<T, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            RepositoryMethodsCache.Average);
        var result = Provider.QueryFirstOrDefault<TResult>(callExpr);
        return result;
    }

    public async Task<TResult> AverageAsync<TResult>(Expression<Func<T, TResult>> selector)
    {
        var callExpr = GetMaxOrMinOrSumOrAverageMethodCallExpression(selector,
            RepositoryMethodsCache.Average);
        var result = await Provider.QueryFirstOrDefaultAsync<TResult>(callExpr);
        return result;
    }

    public int ExecuteUpdate()
    {
        var callExpr = GetExecuteUpdateMethodCallExpression();
        var result = Provider.Execute(callExpr);

        return result;
    }

    public async Task<int> ExecuteUpdateAsync()
    {
        var callExpr = GetExecuteUpdateMethodCallExpression();
        var result = await Provider.ExecuteAsync(callExpr);

        return result;
    }

    private MethodCallExpression GetExecuteUpdateMethodCallExpression()
    {
        var methodInfo = RepositoryMethodsCache.ExecuteUpdate.MakeGenericMethod(typeof(T));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Expression
        );
        return callExpr;
    }
}

public interface IPageLambdaRepository<T> : IOrderLambdaRepository<T>
{
    /// <summary>
    /// 生成经过分页的结果
    /// </summary>
    /// <returns></returns>
    Page<T> ToPage();
    /// <summary>
    /// 生成经过分页的结果
    /// </summary>
    /// <returns></returns>
    Task<Page<T>> ToPageAsync();
}


public class PageLambdaRepository<T> : OrderLambdaRepository<T>, IPageLambdaRepository<T>
{
    public IBaseRepository<T> Source { get; }

    public PageLambdaRepository(IBaseRepository<T> source)
    {
        Source = source;
    }

    public Page<T> ToPage()
    {
        throw new NotImplementedException();
    }

    public async Task<Page<T>> ToPageAsync()
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// SqlExecutor;sql执行器
/// </summary>
public interface ISqlExecutor
{
    #region sync
    /// <summary>
    /// Get list data through sql statement
    /// 通过sql语句获取列表数据
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="sql"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    List<TResult> QueryList<TResult>(string sql, object param = null);
    /// <summary>
    /// Get a single entity through sql statement
    /// 通过sql语句获取单个实体
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="sql"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    TResult QueryFirstOrDefault<TResult>(string sql, object param = null);
    /// <summary>
    /// Execute SQL statements
    /// 执行sql语句
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    int Execute(string sql, object param = null);
    /// <summary>
    /// Paging query through sql statement
    /// 通过sql语句分页查询
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="sql"></param>
    /// <param name="param"></param>
    /// <param name="pageParameter"></param>
    /// <returns></returns>
    Page<TResult> QueryPage<TResult>(string sql, Pageable pageParameter, object param = null);
    #endregion

    #region async
    /// <summary>
    /// Get list data through sql statement
    /// 通过sql语句获取列表数据
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="sql"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    Task<List<TResult>> QueryListAsync<TResult>(string sql, object param = null);
    /// <summary>
    /// Get a single entity through sql statement
    /// 通过sql语句获取单个实体
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="sql"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    Task<TResult> QueryFirstOrDefaultAsync<TResult>(string sql, object param = null);
    /// <summary>
    /// Execute SQL statements
    /// 执行sql语句
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    Task<int> ExecuteAsync(string sql, object param = null);
    /// <summary>
    /// Paging query through sql statement
    /// 通过sql语句分页查询
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="sql"></param>
    /// <param name="param"></param>
    /// <param name="pageParameter"></param>
    /// <returns></returns>
    Task<Page<TResult>> QueryPageAsync<TResult>(string sql, Pageable pageParameter, object param = null);
    #endregion
}

public interface IBaseRepository<T> : IPageLambdaRepository<T>, ISqlExecutor
{
    #region sync
    /// <summary>
    /// Get entity by id
    /// 通过id获得实体
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    T Get(object id);
    /// <summary>
    /// Get all entities
    /// 获取所有实体
    /// </summary>
    /// <returns></returns>
    List<T> GetAll();
    /// <summary>
    /// Update Entity
    /// 更新实体
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    int Update(T t);
    /// <summary>
    /// Update entity list
    /// 更新实体列表
    /// </summary>
    /// <param name="list"></param>
    void Update(List<T> list);
    /// <summary>
    /// Delete Entity
    /// 删除实体
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    int Delete(T t);
    /// <summary>
    /// Delete Entity list
    /// 删除实体列表
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    void Delete(List<T> list);
    /// <summary>
    /// Deleting entities by condition
    /// 通过条件删除实体
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    int Delete(Expression<Func<T, bool>> predicate);
    /// <summary>
    /// Insert Entity
    /// 插入实体
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    T Insert(T t);
    /// <summary>
    /// Insert entity list
    /// 插入实体列表
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    List<T> Insert(List<T> list);
    /// <summary>
    /// Fast bulk insert
    /// 快速批量插入
    /// </summary>
    /// <param name="list"></param>
    void FastBatchInsert(List<T> list);

    #endregion sync

    #region async
    /// <summary>
    /// Get entity by id
    /// 通过id获得实体
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<T> GetAsync(object id);
    /// <summary>
    /// Get all entities
    /// 获取所有实体
    /// </summary>
    /// <returns></returns>
    Task<List<T>> GetAllAsync();
    /// <summary>
    /// Update Entity
    /// 更新实体
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    Task<int> UpdateAsync(T t);
    /// <summary>
    /// Update entity list
    /// 更新实体列表
    /// </summary>
    /// <param name="list"></param>
    Task UpdateAsync(List<T> list);
    /// <summary>
    /// Delete Entity
    /// 删除实体
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    Task<int> DeleteAsync(T t);
    /// <summary>
    /// Deleting entities by condition
    /// 通过条件删除实体
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    Task<int> DeleteAsync(Expression<Func<T, bool>> predicate);
    /// <summary>
    /// Delete Entity list
    /// 删除实体列表
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    Task DeleteAsync(List<T> list);
    /// <summary>
    /// Insert Entity
    /// 插入实体
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    Task<T> InsertAsync(T t);
    /// <summary>
    /// Insert entity list
    /// 插入实体列表
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    Task<List<T>> InsertAsync(List<T> list);
    /// <summary>
    /// Fast bulk insert
    /// 快速批量插入
    /// </summary>
    /// <param name="list"></param>
    Task FastBatchInsertAsync(List<T> list);

    #endregion async
}