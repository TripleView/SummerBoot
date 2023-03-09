using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SummerBoot.Repository.ExpressionParser.Parser
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
        public static IRepository<T> SetValue<T>(this IQueryable<T> source2, Expression<Func<T, object>> select, object value)
        {
            if (!(source2 is IRepository<T> source))
            {
                throw new Exception("only support IRepository");
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

        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source2"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static Page<T> ToPage<T>(this IQueryable<T> source2)
        {
            if (!(source2 is IRepository<T> source))
            {
                throw new Exception("only support IRepository");
            }
            if (source == null) throw new ArgumentNullException("source");
            
            return source.ToPage();
        }

        /// <summary>
        /// 异步分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source2"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<Page<T>> ToPageAsync<T>(this IQueryable<T> source2)
        {
            if (!(source2 is IRepository<T> source))
            {
                throw new Exception("only support IRepository");
            }
            if (source == null) throw new ArgumentNullException("source");

            return  await source.ToPageAsync();
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source2"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static Page<T> ToPage<T>(this IOrderedQueryable<T> source2)
        {
            if (!(source2 is IRepository<T> source))
            {
                throw new Exception("only support IRepository");
            }
            if (source == null) throw new ArgumentNullException("source");

            return source.ToPage();
        }

        /// <summary>
        /// 异步分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source2"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<Page<T>> ToPageAsync<T>(this IOrderedQueryable<T> source2)
        {
            if (!(source2 is IRepository<T> source))
            {
                throw new Exception("only support IRepository");
            }
            if (source == null) throw new ArgumentNullException("source");

            return await source.ToPageAsync();
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source2"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static Page<T> ToPage<T>(this IOrderedQueryable<T> source2,IPageable pageable)
        {
            if (!(source2 is IRepository<T> source))
            {
                throw new Exception("only support IRepository");
            }
            if (source == null) throw new ArgumentNullException("source");

            return source.ToPage(pageable);
        }

        /// <summary>
        /// 异步分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source2"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<Page<T>> ToPageAsync<T>(this IOrderedQueryable<T> source2, IPageable pageable)
        {
            if (!(source2 is IRepository<T> source))
            {
                throw new Exception("only support IRepository");
            }
            if (source == null) throw new ArgumentNullException("source");

            return await source.ToPageAsync(pageable);
        }

        /// <summary>
        /// 异步获取列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source2"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<List<T>> ToListAsync<T>(this IOrderedQueryable<T> source2)
        {
            if (!(source2 is IRepository<T> source))
            {
                throw new Exception("only support IRepository");
            }
            if (source == null) throw new ArgumentNullException("source");

            return await source.ToListAsync();
        }

        /// <summary>
        /// 异步获取列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source2"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<List<T>> ToListAsync<T>(this IQueryable<T> source2)
        {
            if (!(source2 is IRepository<T> source))
            {
                throw new Exception("only support IRepository");
            }
            if (source == null) throw new ArgumentNullException("source");

            return await source.ToListAsync();
        }
    }
}