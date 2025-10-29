using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using SqlParser.Net;
using SummerBoot.Repository.ExpressionParser.Base;
using SummerBoot.Repository.ExpressionParser.Parser.Dialect;
using SummerBoot.Repository.ExpressionParser.Parser.MultiQuery;

namespace SummerBoot.Repository.ExpressionParser.Parser
{
    public class Repository<T> : IRepository<T>
    {
        protected DatabaseUnit databaseUnit;
        private DbType dbType;
        public Type ElementType => typeof(T);
        protected QueryFormatter QueryFormatter;

        public Expression Expression { get; private set; }

        public IQueryProvider Provider { get; private set; }
        public List<SelectItem<T>> SelectItems { get; set; } = new List<SelectItem<T>>();

        public MultiQueryContext<T> MultiQueryContext { get; set; } = new MultiQueryContext<T>();

        public Repository(DatabaseUnit databaseUnit)
        {
            Init(databaseUnit);
        }

        public Repository()
        {

        }

        public Repository(Expression expression, DbQueryProvider provider) : this(provider.DatabaseUnit)
        {
            Provider = provider;
            Expression = expression;
        }


        protected void Init(DatabaseUnit databaseUnit)
        {
            Provider = new DbQueryProvider(databaseUnit, this, GetDbQueryResultByExpression);
            //最后一个表达式将是第一个IQueryable对象的引用。 
            Expression = Expression.Constant(this);
            var databaseType = databaseUnit.DatabaseType;
            this.databaseUnit = databaseUnit;
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    dbType = DbType.SqlServer;
                    this.QueryFormatter = new SqlServerQueryFormatter(databaseUnit);
                    break;
                case DatabaseType.Mysql:
                    dbType = DbType.MySql;
                    this.QueryFormatter = new MysqlQueryFormatter(databaseUnit);
                    break;
                case DatabaseType.Oracle:
                    dbType = DbType.Oracle;
                    this.QueryFormatter = new OracleQueryFormatter(databaseUnit);
                    break;
                case DatabaseType.Sqlite:
                    dbType = DbType.Sqlite;
                    this.QueryFormatter = new SqliteQueryFormatter(databaseUnit);
                    break;
                case DatabaseType.Pgsql:
                    dbType = DbType.Pgsql;
                    this.QueryFormatter = new PgsqlQueryFormatter(databaseUnit);
                    break;
            }
        }

        public virtual Page<TResult> QueryPage<TResult>(string sql, Pageable pageParameter, object param = null)
        {
            return default;
        }

        public virtual Task<Page<TResult>> QueryPageAsync<TResult>(string sql, Pageable pageParameter, object param = null)
        {
            return default;
        }

        public virtual Task<int> ExecuteAsync(string sql, object param = null)
        {
            return default;
        }

        public virtual int Execute(string sql, object param = null)
        {
            return default;
        }

        public virtual TResult QueryFirstOrDefault<TResult>(string sql, object param = null)
        {
            return default;
        }

        public virtual Task<TResult> QueryFirstOrDefaultAsync<TResult>(string sql, object param = null)
        {
            return default;
        }

        public virtual List<TResult> QueryList<TResult>(string sql, object param = null)
        {
            return default;
        }
        public virtual Task<List<TResult>> QueryListAsync<TResult>(string sql, object param = null)
        {
            return default;
        }


        public IEnumerator<T> GetEnumerator()
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var dbParam = this.GetDbQueryResultByExpression(this.Expression);
                var result =  this.QueryList<T>(dbParam.Sql, dbParam.Parameters);
                //var result = dbQueryProvider.linkRepository.QueryList<T>(dbParam.Sql, dbParam.Parameters);

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

        public ExpressionTreeParsingResult GetParsingResult()
        {
            return null;
        }

        protected DbQueryResult InternalInsert(T insertEntity)
        {
            var result = QueryFormatter.Insert(insertEntity);
            return result;
        }

        /// <summary>
        /// 快速批量插入
        /// </summary>
        /// <param name="insertEntities"></param>
        /// <returns></returns>
        protected DbQueryResult InternalFastInsert(List<T> insertEntities)
        {
            return QueryFormatter.FastBatchInsert(insertEntities);
        }

        protected DbQueryResult InternalUpdate(T updateEntity)
        {
            return QueryFormatter.Update(updateEntity); ;
        }

        protected DbQueryResult InternalDelete(T deleteEntity)
        {
            return QueryFormatter.Delete(deleteEntity);
        }

        protected DbQueryResult InternalDelete(Expression predicate)
        {
            return QueryFormatter.DeleteByExpression<T>(predicate);
        }

        protected DbQueryResult InternalGet(dynamic id)
        {
            return QueryFormatter.Get<T>(id);
        }

        protected DbQueryResult InternalGetAll()
        {
            return QueryFormatter.GetAll<T>();
        }

        public int ExecuteUpdate()
        {
            if (this.SelectItems?.Count == 0)
            {
                throw new Exception("set value first");
            }

            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var dbQueryResult = QueryFormatter.ExecuteUpdate(Expression, this.SelectItems);
                return dbQueryProvider.linkRepository.Execute(dbQueryResult.Sql, dbQueryResult.GetDynamicParameters());
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
                var dbQueryResult = QueryFormatter.ExecuteUpdate(Expression, this.SelectItems);
                return await dbQueryProvider.linkRepository.ExecuteAsync(dbQueryResult.Sql, dbQueryResult.GetDynamicParameters());
            }

            return 0;
        }

        public Page<T> ToPage()
        {
            if (Provider is DbQueryProvider dbQueryProvider)
            {
                var dbParam = dbQueryProvider.GetDbPageQueryResultByExpression(Expression);
                var result = dbQueryProvider.linkRepository.QueryPage<T>(dbParam.Sql, null, dbParam.GetDynamicParameters());
                return result;
            }

            return default;
        }

        public async Task<Page<T>> ToPageAsync()
        {
            var newDbExpressionVisitor = new NewDbExpressionVisitor(databaseUnit);
            newDbExpressionVisitor.Visit(Expression);
            var parsingResult = newDbExpressionVisitor.GetParsingResult();

            var result = await this.QueryPageAsync<T>(parsingResult.Sql, null, parsingResult.Parameters);
            return result;

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
            var result = this.Skip((pageable.PageNumber - 1) * pageable.PageSize).Take(pageable.PageSize).ToPage();
            return result;
        }

        public virtual T Get(object id)
        {
            throw new NotImplementedException();
        }

        public virtual List<T> GetAll()
        {
            throw new NotImplementedException();
        }

        public virtual int Update(T t)
        {
            throw new NotImplementedException();
        }

        public virtual void Update(List<T> list)
        {
            throw new NotImplementedException();
        }

        public virtual int Delete(T t)
        {
            throw new NotImplementedException();
        }

        public virtual void Delete(List<T> list)
        {
            throw new NotImplementedException();
        }

        public virtual int Delete(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public virtual T Insert(T t)
        {
            InternalInsert(t);
            return default;
        }

        public virtual List<T> Insert(List<T> list)
        {
            throw new NotImplementedException();
        }

        public virtual void FastBatchInsert(List<T> list)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<T> GetAsync(object id)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public virtual async Task<int> UpdateAsync(T t)
        {
            throw new NotImplementedException();
        }

        public virtual async Task UpdateAsync(List<T> list)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<int> DeleteAsync(T t)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<int> DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public virtual async Task DeleteAsync(List<T> list)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<T> InsertAsync(T t)
        {
            InternalInsert(t);
            return default;
        }

        public virtual async Task<List<T>> InsertAsync(List<T> list)
        {
            throw new NotImplementedException();
        }

        public virtual async Task FastBatchInsertAsync(List<T> list)
        {
            throw new NotImplementedException();
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
                var dbParam = this.GetDbQueryResultByExpression(Expression);
                var result = await dbQueryProvider.linkRepository.QueryListAsync<T>(dbParam.Sql, dbParam.Parameters);
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
                var methodInfo = QueryableMethodsExtension.GetMethodInfoWithSelector(nameof(Queryable.First), true, 1, 2, typeof(bool));
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

        private ExpressionTreeParsingResult GetDbQueryResultByExpression(Expression expression)
        {
            var newDbExpressionVisitor = new NewDbExpressionVisitor(databaseUnit);
            newDbExpressionVisitor.Visit(expression);
            var result = newDbExpressionVisitor.GetParsingResult();
            return result;
        }
    }

}