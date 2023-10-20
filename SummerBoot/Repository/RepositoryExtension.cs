using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SummerBoot.Repository.ExpressionParser.Parser;
using SummerBoot.Repository.ExpressionParser.Parser.MultiQuery;

namespace SummerBoot.Repository
{
    public  static partial class RepositoryExtension
    {
        /// <summary>
        /// 对仓储查出的结果进行赋值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="select"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IRepository<T> SetValue<T>(this IQueryable<T> source, Expression<Func<T, object>> select, object value)
        {
            if (!(source is IRepository<T> repository))
            {
                throw new Exception("only support IRepository");
            }
            if (repository == null) throw new ArgumentNullException("source");
            if (select == null) throw new ArgumentNullException("select");
            repository.SelectItems.Add(new SelectItem<T>()
            {
                Select = select,
                Value = value
            });

            return repository;
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source2"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static Page<T> ToPage<T>(this IQueryable<T> source)
        {
            if (!(source is IRepository<T> repository))
            {
                throw new Exception("only support IRepository");
            }
            if (repository == null) throw new ArgumentNullException("source");

            return repository.ToPage();
        }

        /// <summary>
        /// 异步分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source2"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<Page<T>> ToPageAsync<T>(this IQueryable<T> source)
        {
            if (!(source is IRepository<T> repository))
            {
                throw new Exception("only support IRepository");
            }
            if (repository == null) throw new ArgumentNullException("source");

            return await repository.ToPageAsync();
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static Page<T> ToPage<T>(this IQueryable<T> source, IPageable pageable)
        {
            if (!(source is IRepository<T> repository))
            {
                throw new Exception("only support IRepository");
            }
            if (repository == null) throw new ArgumentNullException("source");

            return repository.ToPage(pageable);
        }

        /// <summary>
        /// 异步分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source2"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<Page<T>> ToPageAsync<T>(this IQueryable<T> source, IPageable pageable)
        {
            if (!(source is IRepository<T> repository))
            {
                throw new Exception("only support IRepository");
            }
            if (repository == null) throw new ArgumentNullException("source");

            return await repository.ToPageAsync(pageable);
        }

        /// <summary>
        /// 异步获取列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source2"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<List<T>> ToListAsync<T>(this IQueryable<T> source)
        {
            if (!(source is IRepository<T> repository))
            {
                throw new Exception("only support IRepository");
            }
            if (repository == null) throw new ArgumentNullException("source");

            return await repository.ToListAsync();
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> source)
        {
            if (!(source is IRepository<T> repository))
            {
                throw new Exception("only support IRepository");
            }
            if (repository == null) throw new ArgumentNullException("source");

            return await repository.FirstOrDefaultAsync();
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> selector)
        {
            if (!(source is IRepository<T> repository))
            {
                throw new Exception("only support IRepository");
            }
            if (repository == null) throw new ArgumentNullException("source");
            var result = await repository.FirstOrDefaultAsync(selector);
            return result;
        }

        public static async Task<T> FirstAsync<T>(this IQueryable<T> source)
        {
            if (!(source is IRepository<T> repository))
            {
                throw new Exception("only support IRepository");
            }
            if (repository == null) throw new ArgumentNullException("source");

            return await repository.FirstAsync();
        }

        public static async Task<T> FirstAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> selector)
        {
            if (!(source is IRepository<T> repository))
            {
                throw new Exception("only support IRepository");
            }
            if (repository == null) throw new ArgumentNullException("source");
            var result = await repository.FirstAsync(selector);
            return result;
        }

        public static async Task<TResult> SumAsync<T, TResult>(this IQueryable<T> source, Expression<Func<T, TResult>> selector)
        {
            if (!(source is IRepository<T> repository))
            {
                throw new Exception("only support IRepository");
            }

            if (repository == null) throw new ArgumentNullException("source");
            var result = await repository.SumAsync(selector);
            return result;
        }

        public static async Task<TResult> AverageAsync<T, TResult>(this IQueryable<T> source, Expression<Func<T, TResult>> selector)
        {
            if (!(source is IRepository<T> repository))
            {
                throw new Exception("only support IRepository");
            }

            if (repository == null) throw new ArgumentNullException("source");
            var result = await repository.AverageAsync(selector);
            return result;
        }

        public static async Task<TResult> MaxAsync<T, TResult>(
            this IQueryable<T> source,
            Expression<Func<T, TResult>> selector)
        {
            if (!(source is IRepository<T> repository))
            {
                throw new Exception("only support IRepository");
            }
            if (repository == null) throw new ArgumentNullException("source");

            return await repository.MaxAsync(selector);
        }

    }
}