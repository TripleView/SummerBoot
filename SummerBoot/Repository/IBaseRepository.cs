using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SummerBoot.Repository
{
    public interface IBaseRepository<T> : ExpressionParser.Parser.IRepository<T> where T : class
    {
      
    }
}