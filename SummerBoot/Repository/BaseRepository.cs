using Dapper;
using ExpressionParser.Base;
using ExpressionParser.Parser;
using SummerBoot.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SummerBoot.Repository
{
    public class BaseRepository<T> : Repository<T>, IBaseRepository<T> where T : class
    {

        public BaseRepository(IUnitOfWork uow, IDbFactory dbFactory, RepositoryOption repositoryOption)
        {
            this.uow = uow;
            this.dbFactory = dbFactory;

            databaseType = DatabaseType.Mysql;

            if (repositoryOption.IsMysql)
            {
                databaseType = DatabaseType.Mysql;
            }

            if (repositoryOption.IsOracle)
            {
                databaseType = DatabaseType.Oracle;
            }


            if (repositoryOption.IsSqlServer)
            {
                databaseType = DatabaseType.SqlServer;
            }

            if (repositoryOption.IsSqlite)
            {
                databaseType = DatabaseType.Sqlite;
            }

            base.Init(databaseType);
        }

        private IUnitOfWork uow;
        private IDbFactory dbFactory;
        protected IDbConnection dbConnection;
        protected IDbTransaction dbTransaction;
        private DatabaseType databaseType;
        private int cmdTimeOut = 1200;

        public override int Execute(DbQueryResult param)
        {
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(param.SqlParameters);
            var result = dbConnection.Execute(param.Sql, dynamicParameters, dbTransaction);
            CloseDb();
            return result;
        }

        public override async Task<int> ExecuteAsync(DbQueryResult param)
        {
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(param.SqlParameters);
            var result =await dbConnection.ExecuteAsync(param.Sql, dynamicParameters, dbTransaction);
            CloseDb();
            return result;
        }

        public override List<TResult> QueryList<TResult>(DbQueryResult param)
        {
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(param.SqlParameters);

            var result = dbConnection.Query<TResult>(param.Sql, dynamicParameters, dbTransaction).ToList();

            CloseDb();
            return result;
        }

        public override TResult Query<TResult>(DbQueryResult param)
        {
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(param.SqlParameters);

            var result = dbConnection.QueryFirst<TResult>(param.Sql, dynamicParameters, dbTransaction);

            CloseDb();
            return result;
            //return result.FirstOrDefault();
        }

        protected void OpenDb()
        {
            dbConnection = uow.ActiveNumber == 0 ? dbFactory.GetDbConnection() : dbFactory.GetDbTransaction().Connection;
            dbTransaction = uow.ActiveNumber == 0 ? null : dbFactory.GetDbTransaction();
        }

        protected void CloseDb()
        {
            if (uow.ActiveNumber == 0)
            {
                dbConnection.Close();
                dbConnection.Dispose();
            }
        }

        #region sync

        public void Update(List<T> list)
        {
            this.uow.BeginTransaction();
            OpenDb();
            foreach (var item in list)
            {
                Update(item);
            }
            CloseDb();
            this.uow.Commit();
        }

        public void Delete(List<T> list)
        {
            this.uow.BeginTransaction();
            OpenDb();
            foreach (var item in list)
            {
                Delete(item);
            }
            CloseDb();
            this.uow.Commit();
        }

        public T Insert(T t)
        {
            var internalResult = InternalInsert(t);

            OpenDb();

            if (databaseType == DatabaseType.Oracle)
            {
                var dynamicParameters = new DynamicParameters(t);
                if (internalResult.IdKeyPropertyInfo != null)
                {
                    dynamicParameters.Add(internalResult.IdName, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
                }
                
                var sql = internalResult.Sql;
                dbConnection.Execute(sql, dynamicParameters, transaction: dbTransaction);

                if (internalResult.IdKeyPropertyInfo != null)
                {
                    var id = dynamicParameters.Get<int>(internalResult.IdName);
                    internalResult.IdKeyPropertyInfo.SetValue(t, Convert.ChangeType(id, internalResult.IdKeyPropertyInfo.PropertyType));
                }
            }
            else if (databaseType == DatabaseType.Sqlite)
            {
                if (internalResult.IdKeyPropertyInfo != null)
                {
                    var sql = internalResult.Sql + ";" + internalResult.LastInsertIdSql;
                    var dynamicParameters = new DynamicParameters(t);
                    dynamicParameters.Add("id", null);
                    var multiResult = dbConnection.QueryMultiple(sql, dynamicParameters, transaction: dbTransaction);
                    var id = multiResult.Read().FirstOrDefault()?.id;

                    if (id != null)
                    {
                        internalResult.IdKeyPropertyInfo.SetValue(t, Convert.ChangeType(id, internalResult.IdKeyPropertyInfo.PropertyType));
                    }
                }
                else
                {
                    dbConnection.Execute(internalResult.Sql, t, transaction: dbTransaction);
                }
            }

            CloseDb();
            return t;
        }

        public List<T> Insert(List<T> list)
        {
            this.uow.BeginTransaction();
            OpenDb();
            foreach (var item in list)
            {
                Insert(item);
            }
            CloseDb();
            this.uow.Commit();

            return list;
        }

        public List<T> GetAll()
        {
            var internalResult = InternalGetAll();
            OpenDb();
            var result = dbConnection.Query<T>(internalResult.Sql).ToList();
            CloseDb();

            return result;
        }

        public void Update(T t)
        {
            var internalResult = InternalUpdate(t);

            OpenDb();
            dbConnection.Execute(internalResult.Sql, t);
            CloseDb();
        }

        public void Delete(T t)
        {
            var internalResult = InternalDelete(t);

            OpenDb();
            dbConnection.Execute(internalResult.Sql, t);
            CloseDb();

        }

        public int Delete(Expression<Func<T, bool>> predicate)
        { 
            var exp= this.Where(predicate).Expression;
            var internalResult = InternalDelete(exp);
            var dynamicParameters = ChangeDynamicParameters(internalResult.SqlParameters);
            OpenDb();
            var result= dbConnection.Execute(internalResult.Sql, dynamicParameters);
            CloseDb();
            return result;
        }

       
        public T Get(dynamic id)
        {
            DbQueryResult internalResult = InternalGet(id);
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(internalResult.SqlParameters);

            var result = dbConnection.QueryFirstOrDefault<T>(internalResult.Sql, dynamicParameters);
            CloseDb();
            return result;
        }

        #endregion sync

        #region async

        public async Task UpdateAsync(List<T> list)
        {
            this.uow.BeginTransaction();
            OpenDb();
            foreach (var item in list)
            {
                await UpdateAsync(item);
            }
            CloseDb();
            this.uow.Commit();

        }

        public async Task DeleteAsync(List<T> list)
        {
            this.uow.BeginTransaction();
            OpenDb();
            foreach (var item in list)
            {
                await DeleteAsync(item);
            }
            CloseDb();
            this.uow.Commit();
        }

        public async Task<T> InsertAsync(T t)
        {
            var internalResult = InternalInsert(t);

            OpenDb();

            if (databaseType == DatabaseType.Oracle)
            {

            }
            else if (databaseType == DatabaseType.Sqlite)
            {
                if (internalResult.IdKeyPropertyInfo != null)
                {
                    var sql = internalResult.Sql + ";" + internalResult.LastInsertIdSql;
                    var dynamicParameters = new DynamicParameters(t);
                    dynamicParameters.Add("id", null);
                    var multiResult = await dbConnection.QueryMultipleAsync(sql, dynamicParameters, transaction: dbTransaction);
                    var id = multiResult.Read().FirstOrDefault()?.id;

                    if (id != null)
                    {
                        internalResult.IdKeyPropertyInfo.SetValue(t, Convert.ChangeType(id, internalResult.IdKeyPropertyInfo.PropertyType));
                    }
                }
                else
                {
                    await dbConnection.ExecuteAsync(internalResult.Sql, t, transaction: dbTransaction);
                }
            }

            CloseDb();
            return t;
        }

        public async Task<List<T>> InsertAsync(List<T> list)
        {
            this.uow.BeginTransaction();
            OpenDb();
            foreach (var item in list)
            {
                await InsertAsync(item);
            }
            CloseDb();
            this.uow.Commit();

            return list;
        }

        public async Task<List<T>> GetAllAsync()
        {
            var internalResult = InternalGetAll();
            OpenDb();
            var result = (await dbConnection.QueryAsync<T>(internalResult.Sql)).ToList();
            CloseDb();

            return result;
        }

        public async Task UpdateAsync(T t)
        {
            var internalResult = InternalUpdate(t);

            OpenDb();
            await dbConnection.ExecuteAsync(internalResult.Sql, t);
            CloseDb();
        }

        public async Task<int> DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            var exp = this.Where(predicate).Expression;
            var internalResult = InternalDelete(exp);
            var dynamicParameters = ChangeDynamicParameters(internalResult.SqlParameters);
            OpenDb();
           var result=  await dbConnection.ExecuteAsync(internalResult.Sql, dynamicParameters);
            CloseDb();
            return result;
        }

        public async Task DeleteAsync(T t)
        {
            var internalResult = InternalDelete(t);

            OpenDb();
            await dbConnection.ExecuteAsync(internalResult.Sql, t);
            CloseDb();

        }

        public async Task<T> GetAsync(dynamic id)
        {
            DbQueryResult internalResult = InternalGet(id);
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(internalResult.SqlParameters);

            var result = await dbConnection.QueryFirstOrDefaultAsync<T>(internalResult.Sql, dynamicParameters);
            CloseDb();
            return result;
        }


        #endregion async

        protected DynamicParameters ChangeDynamicParameters(List<SqlParameter> originSqlParameters)
        {
            if (originSqlParameters == null || originSqlParameters.Count == 0)
            {
                return null;
            }

            var result = new DynamicParameters();
            foreach (var parameter in originSqlParameters)
            {
                result.Add(parameter.ParameterName, parameter.Value);
            }

            return result;
        }

        public void Update(Expression<Func<T, bool>> wherePredicate, Expression<Func<T, object>> setExpression)
        {
            throw new NotImplementedException();
        }
    }
}