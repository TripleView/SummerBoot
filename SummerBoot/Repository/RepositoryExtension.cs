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
    public static class RepositoryExtension
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

        //public static 

        public static JoinBody<T1, T2> LeftJoin<T1, T2>(this IQueryable<T1> source, T2 t2, Expression<Func<JoinCondition<T1, T2>, bool>> condition) where T1:class where T2:class
        {
            if (!(source is IRepository<T1> repository))
            {
                throw new Exception("only support IRepository");
            }

            if (repository == null) throw new ArgumentNullException("source");

            var result = new JoinBody<T1, T2>()
            {
                JoinType = JoinType.LeftJoin,
                Repository = repository,
                Condition = condition,
                JoinTable = typeof(T2),
                JoinTableAlias = nameof(T2)
            };
            repository.JoinItems.Add(result);
            return result;
        }


        public static SelectMultiQueryBody<T1, T2, TResult> Select<T1, T2, TResult>(this JoinBody<T1, T2> source, Expression<Func<JoinCondition<T1, T2>, TResult>> select)
        {
            var result = new SelectMultiQueryBody<T1, T2, TResult>()
            {
                Select = select,
                Source = source
            };
            source.Repository.MultiQuerySelectItem = select;
            return result;
        }

        public static List<TResult> ToList<T1, T2, TResult>(this SelectMultiQueryBody<T1, T2, TResult> selectMultiQueryBody)
        {
            if (selectMultiQueryBody.Source.Repository.Provider is DbQueryProvider dbQueryProvider)
            {
                if (!(selectMultiQueryBody.Source.Repository is IRepository<T1> repository))
                {
                    throw new Exception("only support IRepository");
                }

                var parameter = dbQueryProvider.GetJoinQueryResultByExpression(selectMultiQueryBody.Source.Repository);

                var result = repository.InternalQueryList<TResult>(parameter);
                return result;
            }
            return new List<TResult>();
        }

        public static JoinBody<T1, T2, T3> LeftJoin<T1, T2, T3>(this JoinBody<T1, T2> source, T3 t3, Expression<Func<JoinCondition<T1, T2, T3>, bool>> condition)
        {
            var repository = source.Repository;
            var result = new JoinBody<T1, T2, T3>()
            {
                JoinType = JoinType.LeftJoin,
                Repository = repository,
                Condition = condition
            };
            repository.JoinItems.Add(result);
            return result;

        }

        public static JoinBody<T1, T2, T3, T4> LeftJoin<T1, T2, T3, T4>(this JoinBody<T1, T2, T3> source, T4 t4, Expression<Func<JoinCondition<T1, T2, T3, T4>, bool>> condition)
        {

            var result = new JoinBody<T1, T2, T3, T4>()
            {
                JoinType = JoinType.LeftJoin,
            };

            return result;
        }

    }





    public class SelectMultiQueryBody<T1, T2, TResult>
    {
        public JoinBody<T1, T2> Source { get; set; }
        public Expression<Func<JoinCondition<T1, T2>, TResult>> Select { get; set; }
    }



}