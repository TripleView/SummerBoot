using SqlParser.Net;
using SummerBoot.Repository.MultiQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

namespace SummerBoot.Repository;

public interface IBaseLambdaRepository
{
    Expression Expression { get; }
    Type ElementType { get; }
    IRepositoryProvider Provider { get; }
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

    Page<TResult> QueryPageWithFullSql<TResult>(string pageSql, string countSql, object param = null);
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

    Task<Page<TResult>> QueryPageWithFullSqlAsync<TResult>(string pageSql, string countSql, object param = null);
    #endregion
}


public interface ISetValueProvider<T>
{
    ISetValueStage<T> SetValue(Expression<Func<T, object>> predicate, object value);
}

public interface IToUpdateProvider
{
    int ToUpdate();
    Task<int> ToUpdateAsync();
}

public interface ISetValueStage<T> : ISetValueProvider<T>,IToUpdateProvider
{
 
}


public interface IExecuteProvider<T> : IBaseLambdaRepository
{
    List<T> ToList();

    Task<List<T>> ToListAsync();

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
}


public interface ILambdaRepository<T> : 
    IBaseLambdaRepository,ISetValueStage<T>,ISelectStage<T>,IOrderByStage<T>,IThenByStage<T>,IWhereStage<T>,IGroupByProvider<T>,ISkipStage<T>,ITakeStage<T>
{
    
}
public interface ISelectStage<T> : IExecuteProvider<T>
{

}
public interface ISelectProvider<T> : IBaseLambdaRepository
{
    ISelectStage<T> Select<TResult>(Expression<Func<T, TResult>> selector);
}

public interface IDistinctProvider<T> : IBaseLambdaRepository
{
    IExecuteProvider<T> Distinct();
}

public interface IWhereProvider<T> : IBaseLambdaRepository
{
    IWhereStage<T> Where(Expression<Func<T, bool>> predicate);
    IWhereStage<T> WhereIf(bool condition, Expression<Func<T, bool>> predicate);
}

public interface IWhereStage<T> : IWhereProvider<T>, IOrderByProvider<T>, ISelectProvider<T>, IGroupByProvider<T>,IExecuteProvider<T>
{

}

public interface IOrderByProvider<T> : IBaseLambdaRepository
{
    IOrderByStage<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);
    IOrderByStage<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
}

public interface ISkipProvider<T> : IBaseLambdaRepository
{
    ISkipStage<T>  Skip(int skip);
}

public interface ISkipStage<T> : ITakeProvider<T>
{
    
}

public interface ITakeProvider<T> : IBaseLambdaRepository
{
    ITakeStage<T> Take(int take);
}

public interface ITakeStage<T> : IBaseLambdaRepository,IExecuteProvider<T>
{
    
}

public interface IOrderByStage<T> : IThenByProvider<T>, ISelectProvider<T>,ISkipProvider<T>,ITakeProvider<T>
{

}

public interface IThenByProvider<T> : IBaseLambdaRepository
{
    IThenByStage<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector);

    IThenByStage<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
}

public interface IThenByStage<T> : ISelectProvider<T>, IThenByProvider<T>,ISkipProvider<T>,ITakeProvider<T>
{

}

public interface IHavingProvider<T, TKey>
{
    IHavingStage<T, TKey> Having(Expression<Func<IGrouping<TKey, T>, bool>> predicate);
}

public interface IHavingStage<T, TKey> : IGroupBySelectProvider<T, TKey>
{

}


public interface IGroupByProvider<T>
{
    IGroupByStage<T, TKey> GroupBy<TKey>(Expression<Func<T, TKey>> keySelector);
}


public interface IGroupByStage<T, TKey> : IGroupBySelectProvider<T, TKey>, IHavingProvider<T, TKey>
{

}

public interface IGroupBySelectProvider<T, TKey> : IBaseLambdaRepository
{
    ISelectStage<TResult> Select<TResult>(Expression<Func<IGrouping<TKey, T>, TResult>> selector);
}

public class LambdaRepository<T> : ILambdaRepository<T>
{
    public LambdaRepository(Expression expression, IRepositoryProvider provider)
    {
        this.Expression = expression;
        this.Provider = provider;
    }
    public IRepositoryProvider Provider { get; protected set; }

    public Type ElementType => typeof(T);

    public IExecuteProvider<T> Distinct()
    {
        var methodInfo = RepositoryMethodsCache.Distinct.MakeGenericMethod(typeof(T));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Expression
        );
        return new LambdaRepository<T>(callExpr, Provider);
    }

    public ISelectStage<T> Select<TResult>(Expression<Func<T, TResult>> selector)
    {
        var methodInfo = RepositoryMethodsCache.Select.MakeGenericMethod(typeof(T), typeof(TResult));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Expression,
            Expression.Quote(selector)
        );
        return new LambdaRepository<T>(callExpr, Provider);
    }
    
    public Expression Expression { get; protected set; }

    public ISetValueStage<T> SetValue(Expression<Func<T, object>> predicate, object value)
    {
        var methodInfo = RepositoryMethodsCache.SetValue.MakeGenericMethod(typeof(T));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            this.Expression,
            Expression.Quote(predicate),
            Expression.Constant(value,typeof(object))
        );

        return new LambdaRepository<T>(callExpr, Provider);
    }

    public int ToUpdate()
    {
        var methodInfo = RepositoryMethodsCache.ToUpdate.MakeGenericMethod(typeof(T));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            this.Expression
        );
        var result = Provider.Execute(callExpr);
        return result;
    }

    public async Task<int> ToUpdateAsync()
    {
        var methodInfo = RepositoryMethodsCache.ToUpdate.MakeGenericMethod(typeof(T));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            this.Expression
        );
        var result = await Provider.ExecuteAsync(callExpr);
        return result;
    }

    public IWhereStage<T> Where(Expression<Func<T, bool>> predicate)
    {
        var methodInfo = RepositoryMethodsCache.Where.MakeGenericMethod(typeof(T));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            this.Expression,
            Expression.Quote(predicate)
        );

        return new LambdaRepository<T>(callExpr, Provider);
    }

    public IWhereStage<T> WhereIf(bool condition, Expression<Func<T, bool>> predicate)
    {
        if (condition)
        {
            return Where(predicate);
        }
        return this;
    }

    public IOrderByStage<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        var methodInfo = RepositoryMethodsCache.OrderBy.MakeGenericMethod(typeof(T), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }

    public IOrderByStage<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        var methodInfo = RepositoryMethodsCache.OrderByDescending.MakeGenericMethod(typeof(T), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }

    private LambdaRepository<T> InternalOrderBy<TKey>(
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

        return new LambdaRepository<T>(callExpr, Provider);
    }
    
    private LambdaRepository<T> InternalTakeSkip(
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
    
        return new LambdaRepository<T>(callExpr, Provider);
    }

    public ISkipStage<T> Skip(int skip)
    {
       var methodInfo = RepositoryMethodsCache.Skip.MakeGenericMethod(typeof(T));
       return InternalTakeSkip(skip, methodInfo);
    }

    public ITakeStage<T> Take(int take)
    {
        var methodInfo = RepositoryMethodsCache.Take.MakeGenericMethod(typeof(T));
        return InternalTakeSkip(take, methodInfo);
    }
    
    public IGroupByStage<T, TKey> GroupBy<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        var methodInfo = RepositoryMethodsCache.GroupBy.MakeGenericMethod(typeof(T), typeof(TKey));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Expression,
            Expression.Quote(keySelector)
        );
       return new GroupRepository<T, TKey>(callExpr, Provider);
    }
    
    public IThenByStage<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        var methodInfo = RepositoryMethodsCache.ThenBy.MakeGenericMethod(typeof(T), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
        return result;
    }

    public IThenByStage<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        var methodInfo = RepositoryMethodsCache.ThenByDescending.MakeGenericMethod(typeof(T), typeof(TKey));
        var result = InternalOrderBy(keySelector, methodInfo);
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
        var result = this.FirstOrDefault(predicate);
        if (result == null)
        {
            throw new Exception("Sequence contains no matching element");
        }
        return result;
    }

    public async Task<T> FirstAsync(Expression<Func<T, bool>> predicate)
    {
        var result = await this.FirstOrDefaultAsync(predicate);
        if (result == null)
        {
            throw new Exception("Sequence contains no matching element");
        }
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

    private MethodCallExpression GetFirstOrDefaultMethodCallExpression(Expression<Func<T, bool>> predicate)
    {
        var methodInfo = RepositoryMethodsCache.FirstOrDefaultWithPredicate.MakeGenericMethod(typeof(T));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Expression,
            Expression.Quote(predicate)
        );
        return callExpr;
    }


    public T FirstOrDefault(Expression<Func<T, bool>> predicate)
    {
        var callExpression = GetFirstOrDefaultMethodCallExpression(predicate);
        var result = Provider.QueryFirstOrDefault<T>(callExpression);
        return result;
    }

    public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        var callExpression = GetFirstOrDefaultMethodCallExpression(predicate);
        var result = await Provider.QueryFirstOrDefaultAsync<T>(callExpression);
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
    
    public List<T> ToList()
    {
        var result = Provider.QueryList<T>(Expression);
        return result;
    }

    public async Task<List<T>> ToListAsync()
    {
        var result = await Provider.QueryListAsync<T>(Expression);
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
        var result = Provider.QueryPage<T>(callExpr);

        return result;
    }

    public async Task<Page<T>> ToPageAsync()
    {
        var callExpr = GetToPageMethodCallExpression();
        var result = await Provider.QueryPageAsync<T>(callExpr);

        return result;
    }
    
 
}

public class GroupRepository<T, TKey> : IGroupByStage<T, TKey>, IBaseLambdaRepository,IHavingStage<T, TKey>
{
    public GroupRepository(Expression expression, IRepositoryProvider provider)
    {
        this.Expression = expression;
        this.Provider = provider;
    }
    public IRepositoryProvider Provider { get; protected set; }

    public Type ElementType => typeof(T);

    public ISelectStage<TResult> Select<TResult>(Expression<Func<IGrouping<TKey, T>, TResult>> selector)
    {
        var methodInfo = RepositoryMethodsCache.GroupBySelect.MakeGenericMethod(typeof(T), typeof(TKey), typeof(TResult));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Expression,
            Expression.Quote(selector)
        );

        return new LambdaRepository<TResult>(callExpr, Provider);
    }

    public Expression Expression { get; protected set; }
    
    public IHavingStage<T, TKey> Having(Expression<Func<IGrouping<TKey, T>, bool>> predicate)
    {
        var methodInfo = RepositoryMethodsCache.Having.MakeGenericMethod(typeof(T), typeof(TKey));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Expression,
            Expression.Quote(predicate)
        );

        return new GroupRepository<T, TKey>(callExpr, Provider);
    }
}

public interface IBaseRepository<T> :ILambdaRepository<T>, ISqlExecutor
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