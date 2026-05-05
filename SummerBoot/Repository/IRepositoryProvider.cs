using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using SummerBoot.Repository.ExpressionParser;

namespace SummerBoot.Repository;

public interface IRepositoryProvider
{
    T CreateQuery<T>(Expression expression);
    int Execute(Expression expression);

    Task<int> ExecuteAsync(Expression expression);

    TResult QueryFirstOrDefault<TResult>(Expression expression);

    Task<TResult> QueryFirstOrDefaultAsync<TResult>(Expression expression);

    List<TResult> QueryList<TResult>(Expression expression);

    Task<List<TResult>> QueryListAsync<TResult>(Expression expression);

    Page<TResult> QueryPage<TResult>(Expression expression);

    Task<Page<TResult>> QueryPageAsync<TResult>(Expression expression);
}

public class RepositoryProvider : IRepositoryProvider
{
    public DatabaseUnit DatabaseUnit;
    public ISqlExecutor Repository;
    public RepositoryProvider(DatabaseUnit databaseUnit, ISqlExecutor repository)
    {
        DatabaseUnit = databaseUnit;
        this.Repository = repository;
    }
    public T CreateQuery<T>(Expression expression)
    {
        var type = typeof(T);
        if (type.IsGenericType)
        {
            var genericType = type.GetGenericTypeDefinition();
            var childrenType = type.GetGenericArguments().First();
            Type newType = null;
            if (genericType == typeof(IOrderLambdaRepository<>))
            {
                newType = typeof(OrderLambdaRepository<>).MakeGenericType(childrenType);
            }
            else if (genericType == typeof(IPageLambdaRepository<>))
            {
                newType = typeof(PageLambdaRepository<>).MakeGenericType(childrenType);
            }
            else if (genericType == typeof(IBaseRepository<>))
            {
                newType = typeof(CustomBaseRepository<>).MakeGenericType(childrenType);
            }

            var result = (T)Activator.CreateInstance(newType);
            return result;
        }
        else
        {
            throw new NotSupportedException(typeof(T).FullName);
        }
    }

    public int Execute(Expression expression)
    {
        var wrapperExpression = GetDbQueryResultByExpression(expression);
        var sql = wrapperExpression.SqlExpression.ToSql();
        var parameters = wrapperExpression.Parameters;
        return Repository.Execute(sql, parameters);
    }

    public async Task<int> ExecuteAsync(Expression expression)
    {
        var wrapperExpression = GetDbQueryResultByExpression(expression);
        var sql = wrapperExpression.SqlExpression.ToSql();
        var parameters = wrapperExpression.Parameters;
        return await Repository.ExecuteAsync(sql, parameters);
    }

    public TResult QueryFirstOrDefault<TResult>(Expression expression)
    {
        var wrapperExpression = GetDbQueryResultByExpression(expression);
        var sql = wrapperExpression.SqlExpression.ToSql();
        var parameters = wrapperExpression.Parameters;
        return Repository.QueryFirstOrDefault<TResult>(sql, parameters);
    }

    public async Task<TResult> QueryFirstOrDefaultAsync<TResult>(Expression expression)
    {
        var wrapperExpression = GetDbQueryResultByExpression(expression);
        var sql = wrapperExpression.SqlExpression.ToSql();
        var parameters = wrapperExpression.Parameters;
        return await Repository.QueryFirstOrDefaultAsync<TResult>(sql, parameters);
    }

    private WrapperExpression GetDbQueryResultByExpression(Expression expression)
    {
        var newDbExpressionVisitor = new NewDbExpressionVisitor(DatabaseUnit);
        var exp = newDbExpressionVisitor.Visit(expression);
        if (exp is WrapperExpression wrapperExpression)
        {
            return wrapperExpression;
        }

        throw new NotSupportedException(expression.ToString());
    }

    public List<TResult> QueryList<TResult>(Expression expression)
    {
        var wrapperExpression = GetDbQueryResultByExpression(expression);
        var sql = wrapperExpression.SqlExpression.ToSql();
        var parameters = wrapperExpression.Parameters;
        return Repository.QueryList<TResult>(sql, parameters);
    }

    public async Task<List<TResult>> QueryListAsync<TResult>(Expression expression)
    {
        var wrapperExpression = GetDbQueryResultByExpression(expression);
        var sql = wrapperExpression.SqlExpression.ToSql();
        var parameters = wrapperExpression.Parameters;
        return await Repository.QueryListAsync<TResult>(sql, parameters);
    }

    public Page<TResult> QueryPage<TResult>(Expression expression)
    {
        var wrapperExpression = GetDbQueryResultByExpression(expression);
        var sql = wrapperExpression.SqlExpression.ToSql();
        var parameters = wrapperExpression.Parameters;
        return Repository.QueryPage<TResult>(sql, null, parameters);
    }

    public async Task<Page<TResult>> QueryPageAsync<TResult>(Expression expression)
    {
        var wrapperExpression = GetDbQueryResultByExpression(expression);
        var sql = wrapperExpression.SqlExpression.ToSql();
        var parameters = wrapperExpression.Parameters;
        return await Repository.QueryPageAsync<TResult>(sql, null, parameters);
    }
}