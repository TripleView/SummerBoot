using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using SummerBoot.Repository.ExpressionParser;

namespace SummerBoot.Repository;

public interface IRepositoryProvider<TElement>
{
    INewRepository<T> CreateQuery<T>(Expression expression);
    TResult Execute<TResult>(Expression expression);
    List<TResult> QueryList<TResult>(Expression expression);

    Task<List<TResult>> QueryListAsync<TResult>(Expression expression);
}

public class RepositoryProvider<TElement> : IRepositoryProvider<TElement>
{
    public DatabaseUnit DatabaseUnit;
    public INewRepository<TElement> Repository;
    public RepositoryProvider(DatabaseUnit databaseUnit, INewRepository<TElement> repository)
    {
        DatabaseUnit = databaseUnit;
        this.Repository = repository;
    }
    public INewRepository<T> CreateQuery<T>(Expression expression)
    {
        var result = new OrderLambdaRepository<T>();
        result.SetExpressionAndProvider(expression,this);
        return result;
    }

    public TResult Execute<TResult>(Expression expression)
    {
        throw new System.NotImplementedException();
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
}