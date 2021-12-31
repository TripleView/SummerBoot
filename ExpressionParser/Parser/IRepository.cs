using System;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressionParser.Parser
{
    public interface IRepository<T> : IOrderedQueryable<T>
    {
    }
}