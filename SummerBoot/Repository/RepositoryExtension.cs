using SummerBoot.Repository.ExpressionParser.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using SummerBoot.Repository.MultiQuery;

namespace SummerBoot.Repository
{
    public static partial class RepositoryExtension
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
            if (!(source is Repository<T> repository))
            {
                throw new Exception("only support Repository");
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

        public static IQueryable<T> OrWhere<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)
        {
            if (!(source is IRepository<T> repository))
            {
                throw new Exception("only support IRepository");
            }

            if (repository == null) throw new ArgumentNullException("source");
            var methodInfo = new Func<IQueryable<object>, Expression<Func<object, bool>>, IQueryable<object>>(QueryableMethodsExtension.OrWhere)
                .GetMethodInfo().GetGenericMethodDefinition().MakeGenericMethod(typeof(T));

            return repository.Provider.CreateQuery<T>(
                Expression.Call(
                    null,
                    methodInfo,
                    repository.Expression, Expression.Quote(predicate)
                ));

        }

        public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> predicate)
        {
            if (condition)
            {
                return source.Where(predicate);
            }

            return source;
        }

        public static IQueryable<T> OrWhereIf<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> predicate)
        {
            if (condition)
            {
                return source.OrWhere(predicate);
            }

            return source;
        }

        public static IQueryable<JoinTuple<T1, T2>> LeftJoin<T1, T2>(
            this IRepository<T1> source,
            IRepository<T2> joinTable,
            Expression<Func<JoinTuple<T1, T2>, bool>> on) where T1 : new() where T2 : new()
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (joinTable == null) throw new ArgumentNullException(nameof(joinTable));
            if (on == null) throw new ArgumentNullException(nameof(on));

            // 构造 LeftJoin 的表达式树
            var leftJoinMethod = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T1), typeof(T2));
            var callExpr = Expression.Call(
                null,
                leftJoinMethod,
                source.Expression,
                joinTable.Expression,
                Expression.Quote(on)
            );

            // 让 source.Provider 创建新的 IQueryable<JoinTuple<T1, T2>>
            return source.Provider.CreateQuery<JoinTuple<T1, T2>>(callExpr);

        }
    }
}