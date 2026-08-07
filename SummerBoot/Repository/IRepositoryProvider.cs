using SummerBoot.Repository.Core;
using SummerBoot.Repository.ExpressionParser;
using SummerBoot.Repository.ExpressionParser.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Xml.Linq;

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

            if (genericType == typeof(IPageLambdaRepository<>))
            {
                newType = typeof(PageLambdaRepository<>).MakeGenericType(childrenType);

            }
            else if ( genericType == typeof(ILambdaRepository<>) || genericType == typeof(IOrderLambdaRepository<>))
            {
                newType = typeof(OrderLambdaRepository<>).MakeGenericType(childrenType);
            }
            else if (genericType == typeof(IBaseRepository<>))
            {
                newType = typeof(CustomBaseRepository<>).MakeGenericType(childrenType);
            }
            else if (genericType == typeof(IUpdateLambdaRepository<>))
            {
                newType = typeof(UpdateLambdaRepository<>).MakeGenericType(childrenType);
            }
            return (T)Activator.CreateInstance(newType, args: new object[2] { expression, this });
        }
        else
        {
            throw new NotSupportedException(typeof(T).FullName);
        }
    }

    private void LogSql(SqlLogContext sqlLogContext)
    {
        DatabaseUnit.OnLogSqlInfo(sqlLogContext);
    }

    public int Execute(Expression expression)
    {
        var wrapperExpression = GetDbQueryResultByExpression(expression);
        var sql = wrapperExpression.SqlExpression.ToSql();
        var parameters = wrapperExpression.Parameters;
        LogSql(new SqlLogContext() { Sql = sql, Parameters = parameters });
        return Repository.Execute(sql, parameters);
    }

    public async Task<int> ExecuteAsync(Expression expression)
    {
        var wrapperExpression = GetDbQueryResultByExpression(expression);
        var sql = wrapperExpression.SqlExpression.ToSql();
        var parameters = wrapperExpression.Parameters;
        LogSql(new SqlLogContext() { Sql = sql, Parameters = parameters });
        return await Repository.ExecuteAsync(sql, parameters);
    }

    public TResult QueryFirstOrDefault<TResult>(Expression expression)
    {
        var wrapperExpression = GetDbQueryResultByExpression(expression);
        var sql = wrapperExpression.SqlExpression.ToSql();
        var parameters = wrapperExpression.Parameters;
        LogSql(new SqlLogContext() { Sql = sql, Parameters = parameters });
        return Repository.QueryFirstOrDefault<TResult>(sql, parameters);
    }

    public async Task<TResult> QueryFirstOrDefaultAsync<TResult>(Expression expression)
    {
        var wrapperExpression = GetDbQueryResultByExpression(expression);
        var sql = wrapperExpression.SqlExpression.ToSql();
        var parameters = wrapperExpression.Parameters;
        LogSql(new SqlLogContext() { Sql = sql, Parameters = parameters });
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
        LogSql(new SqlLogContext() { Sql = sql, Parameters = parameters });
        return Repository.QueryList<TResult>(sql, parameters);
    }

    public async Task<List<TResult>> QueryListAsync<TResult>(Expression expression)
    {
        var wrapperExpression = GetDbQueryResultByExpression(expression);
        var sql = wrapperExpression.SqlExpression.ToSql();
        var parameters = wrapperExpression.Parameters;
        LogSql(new SqlLogContext() { Sql = sql, Parameters = parameters });
        return await Repository.QueryListAsync<TResult>(sql, parameters);
    }

    public Page<TResult> QueryPage<TResult>(Expression expression)
    {
        var wrapperExpression = GetDbQueryResultByExpression(expression);
        var pageSql = wrapperExpression.SqlExpression.ToSql();
        var countSql = wrapperExpression.CountSqlExpression.ToSql();
        var parameters = wrapperExpression.Parameters;
        LogSql(new SqlLogContext() { Sql = pageSql, Parameters = parameters,CountSql = countSql});
        return Repository.QueryPageWithFullSql<TResult>(pageSql, countSql, parameters);
    }

    public async Task<Page<TResult>> QueryPageAsync<TResult>(Expression expression)
    {
        var wrapperExpression = GetDbQueryResultByExpression(expression);
        var pageSql = wrapperExpression.SqlExpression.ToSql();
        var countSql = wrapperExpression.CountSqlExpression.ToSql();
        var parameters = wrapperExpression.Parameters;
        LogSql(new SqlLogContext() { Sql = pageSql, Parameters = parameters, CountSql = countSql });
        return await Repository.QueryPageWithFullSqlAsync<TResult>(pageSql, countSql, parameters);
    }
}