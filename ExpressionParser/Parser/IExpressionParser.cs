using System;
using System.Linq.Expressions;

namespace ExpressionParser.Parser
{
    public interface IExpressionParser<T> where T : class
    {
        IExpressionParser<T> Select(Expression<Func<T,object>> expression);
        IExpressionParser<T> Where(Expression<Func<T, bool>> expression);
        string GenerateSql();
    }
}