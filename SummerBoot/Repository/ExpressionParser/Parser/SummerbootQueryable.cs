using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SummerBoot.Repository.ExpressionParser.Parser;

public class SummerbootQueryable<T> : IQueryable<T>
{
    private readonly Expression expression;
    public DbQueryProvider DbQueryProvider;

    public SummerbootQueryable(DbQueryProvider provider)
    {
        this.DbQueryProvider = provider;
        this.expression = Expression.Constant(this);
    }

    public SummerbootQueryable(Expression expression, DbQueryProvider provider)
    {
        this.DbQueryProvider = provider;
        this.expression = expression;
    }

    public Type ElementType => typeof(T);
    public Expression Expression => expression;
    public IQueryProvider Provider => DbQueryProvider;

    public IEnumerator<T> GetEnumerator()
    {
        return DbQueryProvider.QueryList<T>(expression).GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}