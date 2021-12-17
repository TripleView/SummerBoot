using System.Linq;

namespace ExpressionParser.Parser
{
    public interface IRepository<T> : IOrderedQueryable<T>
    {
    }
}