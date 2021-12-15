using System.Linq;

namespace DatabaseParser.ExpressionParser
{
    public interface IRepository<T> : IOrderedQueryable<T>
    {
    }
}