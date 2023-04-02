using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using SummerBoot.Repository.ExpressionParser.Base;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public class Repository<T> : IRepository<T>
    {
        public Repository(DatabaseUnit databaseUnit)
        {
            Provider = new DbQueryProvider(databaseUnit, this);
            //最后一个表达式将是第一个IQueryable对象的引用。 
            Expression = Expression.Constant(this);
        }

        public Repository()
        {

        }

        public void Init(DatabaseUnit databaseUnit)
        {
            Provider = new DbQueryProvider(databaseUnit, this);
            //最后一个表达式将是第一个IQueryable对象的引用。 
            Expression = Expression.Constant(this);
        }

        public virtual Task<int> InternalExecuteAsync(DbQueryResult param)
        {
            return default;
        }

        public virtual int InternalExecute(DbQueryResult param)
        {
            return default;
        }

        public virtual Task<Page<TResult>> InternalQueryPageAsync<TResult>(DbQueryResult param)
        {
            return default;
        }

        public virtual Page<TResult> InternalQueryPage<TResult>(DbQueryResult param)
        {
            return default;
        }

        public virtual TResult InternalQuery<TResult>(DbQueryResult param)
        {
            return default;
        }

        public virtual Task<TResult> InternalQueryAsync<TResult>(DbQueryResult param)
        {
            return default;
        }

        public virtual List<TResult> InternalQueryList<TResult>(DbQueryResult param)
        {
            return default;
        }
        public virtual Task<List<TResult>> InternalQueryListAsync<TResult>(DbQueryResult param)
        {
            return default;
        }
        public Repository(Expression expression, IQueryProvider provider)
        {
            Provider = provider;
            Expression = expression;
        }


        public Type ElementType => typeof(T);

        public Expression Expression { get; private set; }


        public IQueryProvider Provider { get; private set; }
        public List<SelectItem<T>> SelectItems { get; set; } = new List<SelectItem<T>>();

        public IEnumerator<T> GetEnumerator()
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var dbParam = dbQueryProvider.GetDbQueryResultByExpression(Expression);
                var result = dbQueryProvider.linkRepository.InternalQueryList<T>(dbParam);
                //var result = new List<T>();
                if (result == null)
                    yield break;
                foreach (var item in result)
                {
                    yield return item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.Provider.Execute(this.Expression)).GetEnumerator();
        }

        public DbQueryResult GetDbQueryDetail()
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                return dbQueryProvider.GetDbQueryDetail();
            }

            return null;
        }

        public DbQueryResult InternalInsert(T insertEntity)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                return dbQueryProvider.queryFormatter.Insert(insertEntity); ;
            }

            return null;
        }

        /// <summary>
        /// 快速批量插入
        /// </summary>
        /// <param name="insertEntity"></param>
        /// <returns></returns>
        public DbQueryResult InternalFastInsert(List<T> insertEntitys)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                return dbQueryProvider.queryFormatter.FastBatchInsert(insertEntitys); ;
            }

            return null;
        }

        public DbQueryResult InternalUpdate(T updateEntity)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                return dbQueryProvider.queryFormatter.Update(updateEntity); ;
            }
            return null;
        }

        public DbQueryResult InternalDelete(T deleteEntity)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                return dbQueryProvider.queryFormatter.Delete(deleteEntity);
            }
            return null;
        }

        public DbQueryResult InternalDelete(Expression predicate)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                return dbQueryProvider.queryFormatter.DeleteByExpression<T>(predicate);
            }
            return null;
        }

        public DbQueryResult InternalGet(dynamic id)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                return dbQueryProvider.queryFormatter.Get<T>(id);
            }
            return null;
        }

        public DbQueryResult InternalGetAll()
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                return dbQueryProvider.queryFormatter.GetAll<T>();
            }
            return null;
        }

        public int ExecuteUpdate()
        {
            if (this.SelectItems?.Count == 0)
            {
                throw new Exception("set value first");
            }

            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var dbQueryResult = dbQueryProvider.queryFormatter.ExecuteUpdate(Expression, this.SelectItems);
                return dbQueryProvider.linkRepository.InternalExecute(dbQueryResult);
            }

            return 0;
        }

        public async Task<int> ExecuteUpdateAsync()
        {
            if (this.SelectItems?.Count == 0)
            {
                throw new Exception("set value first");
            }

            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var dbQueryResult = dbQueryProvider.queryFormatter.ExecuteUpdate(Expression, this.SelectItems);
                return await dbQueryProvider.linkRepository.InternalExecuteAsync(dbQueryResult);
            }

            return 0;
        }

        public Page<T> ToPage()
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var dbParam = dbQueryProvider.GetDbPageQueryResultByExpression(Expression);
                var result = dbQueryProvider.linkRepository.InternalQueryPage<T>(dbParam);
                return result;
            }

            return default;
        }

        public async Task<Page<T>> ToPageAsync()
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var dbParam = dbQueryProvider.GetDbPageQueryResultByExpression(Expression);
                var result = await dbQueryProvider.linkRepository.InternalQueryPageAsync<T>(dbParam);
                return result;
            }
            return default;
        }

        private void CheckPageable(IPageable pageable)
        {
            if (pageable.PageNumber < 0)
            {
                throw new Exception("PageNumber cannot be less than 0");
            }

            if (pageable.PageSize < 0)
            {
                throw new Exception("PageSize cannot be less than 0");
            }
        }

        public Page<T> ToPage(IPageable pageable)
        {
            CheckPageable(pageable);
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var result = this.Skip((pageable.PageNumber - 1) * pageable.PageSize).Take(pageable.PageSize).ToPage();
                return result;
            }
            return default;
        }


        public async Task<Page<T>> ToPageAsync(IPageable pageable)
        {
            CheckPageable(pageable);
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var result = await this.Skip((pageable.PageNumber - 1) * pageable.PageSize).Take(pageable.PageSize).ToPageAsync();
                return result;
            }
            return default;
        }

        public async Task<List<T>> ToListAsync()
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var dbParam = dbQueryProvider.GetDbQueryResultByExpression(Expression);
                var result = await dbQueryProvider.linkRepository.InternalQueryListAsync<T>(dbParam);
                return result;
            }
            return default;
        }

        public async Task<T> FirstOrDefaultAsync()
        {

            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var result = await dbQueryProvider.ExecuteAsync<T>(Expression);
                return result;
            }
            return default;
        }

        public async Task<T> FirstAsync()
        {

            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var result = await dbQueryProvider.ExecuteAsync<T>(Expression);
                if (result == null)
                {
                    throw new Exception("can not find any element");
                }
                return result;
            }
            return default;
        }

        public async Task<TResult> MaxAsync<TResult>(
            Expression<Func<T, TResult>> selector)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var result = await InternalExecuteWithSelectorAsync(nameof(Queryable.Max), selector);
                return result;
            }
            return default;
        }

        public async Task<TResult> MinAsync<TResult>(
            Expression<Func<T, TResult>> selector)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var result = await InternalExecuteWithSelectorAsync(nameof(Queryable.Min), selector);
                return result;
            }
            return default;
        }

        public async Task<TResult> SumAsync<TResult>(
            Expression<Func<T, TResult>> selector)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var result = await InternalSumAvgExecuteWithSelectorAsync(nameof(Queryable.Sum), selector);
                return result;
            }
            return default;
        }

        public async Task<TResult> AverageAsync<TResult>(
            Expression<Func<T, TResult>> selector)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var result = await InternalSumAvgExecuteWithSelectorAsync(nameof(Queryable.Average), selector);
                return result;
            }
            return default;
        }

        public async Task<int> CountAsync(
            Expression<Func<T, bool>> selector)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var methodInfo = QueryableMethodsExtension.GetMethodInfoWithSelector(nameof(Queryable.Count), true, 1, 2);
                methodInfo = methodInfo.MakeGenericMethod(new Type[] { typeof(T) });

                var newSelector = Expression.Quote(selector);
                var callExpression = Expression.Call(null, methodInfo, new[] { this.Expression, newSelector });

                var result = await dbQueryProvider.ExecuteAsync<int>(callExpression);
                return result;
            }
            return default;
        }

        public async Task<T> FirstOrDefaultAsync(
            Expression<Func<T, bool>> selector)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var methodInfo = QueryableMethodsExtension.GetMethodInfoWithSelector(nameof(Queryable.FirstOrDefault), true, 1, 2, typeof(bool));
                methodInfo = methodInfo.MakeGenericMethod(new Type[] { typeof(T) });

                var newSelector = Expression.Quote(selector);
                var callExpression = Expression.Call(null, methodInfo, new[] { this.Expression, newSelector });

                var result = await dbQueryProvider.ExecuteAsync<T>(callExpression);
                return result;
            }
            return default;
        }

        public async Task<T> FirstAsync(
            Expression<Func<T, bool>> selector)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var methodInfo = QueryableMethodsExtension.GetMethodInfoWithSelector(nameof(Queryable.First), true, 1, 2,typeof(bool));
                methodInfo = methodInfo.MakeGenericMethod(new Type[] { typeof(T) });

                var newSelector = Expression.Quote(selector);
                var callExpression = Expression.Call(null, methodInfo, new[] { this.Expression, newSelector });

                var result = await dbQueryProvider.ExecuteAsync<T>(callExpression);
                if (result == null)
                {
                    throw new Exception("can not find any element");
                }
                return result;
            }
            return default;
        }

        private async Task<TResult> InternalExecuteWithSelectorAsync<TResult>(
            string methodName, Expression<Func<T, TResult>> selector)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var methodInfo = QueryableMethodsExtension.GetMethodInfoWithSelector(methodName);
                methodInfo = methodInfo.MakeGenericMethod(new Type[] { typeof(T), typeof(TResult) });

                var newSelector = Expression.Quote(selector);
                var callExpression = Expression.Call(null, methodInfo, new[] { this.Expression, newSelector });

                var result = await dbQueryProvider.ExecuteAsync<TResult>(callExpression);
                return result;
            }
            return default;
        }
        private async Task<TResult> InternalSumAvgExecuteWithSelectorAsync<TResult>(
            string methodName, Expression<Func<T, TResult>> selector)
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {

                var methodInfo = QueryableMethodsExtension.GetSumAvgWithSelector(methodName, typeof(TResult));
                methodInfo = methodInfo.MakeGenericMethod(new Type[] { typeof(T) });

                var newSelector = Expression.Quote(selector);
                var callExpression = Expression.Call(null, methodInfo, new[] { this.Expression, newSelector });

                var result = await dbQueryProvider.ExecuteAsync<TResult>(callExpression);
                return result;
            }
            return default;
        }
    }

}