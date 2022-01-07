using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressionParser.Parser
{
    public static class RepositoryExtension
    {
        /// <summary>
        /// 对仓储查出的结果进行赋值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source2"></param>
        /// <param name="select"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IRepository<T> SetValue<T>(this IQueryable<T> source2, Expression<Func<T, object>> select,object value)
        {
            if (!(source2 is IRepository<T> source))
            {
                throw new Exception("123");
            }
            if(source==null) throw new ArgumentNullException("source");
            if(select==null) throw new ArgumentNullException("select"); 
            source.SelectItems.Add(new SelectItem<T>()
            {
                Select = select,
                Value = value
            });

            return source;
        }

       
    }
}