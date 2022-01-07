using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ExpressionParser.Parser
{
    public interface IRepository<T> : IOrderedQueryable<T>
    {
        List<SelectItem<T>> SelectItems { set; get; }
        void ExecuteUpdate();
        Task ExecuteUpdateAsync();
    }
}