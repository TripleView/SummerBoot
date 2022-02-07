using Dapper;
using SummerBoot.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DatabaseType = SummerBoot.Repository.DatabaseType;
using DbQueryResult = SummerBoot.Repository.ExpressionParser.Parser.DbQueryResult;
using SqlParameter = SummerBoot.Repository.ExpressionParser.Parser.SqlParameter;

namespace SummerBoot.Repository
{
    public class BaseRepository<T> : ExpressionParser.Parser.Repository<T>, IBaseRepository<T> where T : class
    {

        public BaseRepository(IUnitOfWork uow, IDbFactory dbFactory, RepositoryOption repositoryOption)
        {
            this.uow = uow;
            this.dbFactory = dbFactory;
            this.repositoryOption = repositoryOption;

            databaseType = repositoryOption.DatabaseType;

            base.Init(databaseType);
        }

        protected IUnitOfWork uow;
        protected IDbFactory dbFactory;
        protected IDbConnection dbConnection;
        protected IDbTransaction dbTransaction;
        protected DatabaseType databaseType;
        protected int cmdTimeOut = 1200;
        protected RepositoryOption repositoryOption;
        public override int InternalExecute(DbQueryResult param)
        {
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(param.SqlParameters);
            var result = dbConnection.Execute(param.Sql, dynamicParameters, dbTransaction);
            CloseDb();
            return result;
        }

        public override async Task<int> InternalExecuteAsync(DbQueryResult param)
        {
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(param.SqlParameters);
            var result = await dbConnection.ExecuteAsync(param.Sql, dynamicParameters, dbTransaction);
            CloseDb();
            return result;
        }

        public override Page<TResult> InternalQueryPage<TResult>(DbQueryResult param)
        {
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(param.SqlParameters);

            var count = dbConnection.QueryFirst<int>(param.CountSql, dynamicParameters, dbTransaction);
            var item = dbConnection.Query<TResult>(param.Sql, dynamicParameters, dbTransaction);
            CloseDb();
            var result = new Page<TResult>()
            {
                Data = item.ToList(),
                TotalPages = count
            };
            return result;
        }

        public override async Task<Page<TResult>> InternalQueryPageAsync<TResult>(DbQueryResult param)
        {

            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(param.SqlParameters);

            var count = await dbConnection.QueryFirstAsync<int>(param.CountSql, dynamicParameters, dbTransaction);
            var item = await dbConnection.QueryAsync<TResult>(param.Sql, dynamicParameters, dbTransaction);
            CloseDb();
            var result = new Page<TResult>()
            {
                Data = item.ToList(),
                TotalPages = count
            };
            return result;
        }


        public override List<TResult> InternalQueryList<TResult>(DbQueryResult param)
        {
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(param.SqlParameters);

            var result = dbConnection.Query<TResult>(param.Sql, dynamicParameters, dbTransaction).ToList();

            CloseDb();
            return result;
        }

        /// <summary>
        /// 查询列表
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<TResult> QueryList<TResult>(string sql, object param = null)
        {
            OpenDb();

            var result = dbConnection.Query<TResult>(sql, param, dbTransaction).ToList();

            CloseDb();
            return result;
        }

        /// <summary>
        /// 查询列表
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<List<TResult>> QueryListAsync<TResult>(string sql, object param = null)
        {
            OpenDb();
            var result = (await dbConnection.QueryAsync<TResult>(sql, param, dbTransaction)).ToList();
            CloseDb();
            return result;
        }

        /// <summary>
        /// 查询单个结果
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public TResult QueryFirstOrDefault<TResult>(string sql, object param = null)
        {
            OpenDb();

            var result = dbConnection.QueryFirstOrDefault<TResult>(sql, param, dbTransaction);

            CloseDb();
            return result;
        }

        /// <summary>
        /// 查询单个结果
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<TResult> QueryFirstOrDefaultAsync<TResult>(string sql, object param = null)
        {
            OpenDb();

            var result = await dbConnection.QueryFirstOrDefaultAsync<TResult>(sql, param, dbTransaction);

            CloseDb();
            return result;
        }

        /// <summary>
        /// 执行语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public int Execute(string sql, object param = null)
        {
            OpenDb();
            var result = dbConnection.Execute(sql, param, dbTransaction);
            CloseDb();
            return result;
        }

        /// <summary>
        /// 执行语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<int> ExecuteAsync(string sql, object param = null)
        {
            OpenDb();
            var result = await dbConnection.ExecuteAsync(sql, param, dbTransaction);
            CloseDb();
            return result;
        }

        public override TResult InternalQuery<TResult>(DbQueryResult param)
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
            if (repositoryOption.AutoAddCreateOn)
            {

                if (t is BaseEntity baseEntity)
                {
                    baseEntity.CreateOn = repositoryOption.AutoAddCreateOnUseUtc ? DateTime.UtcNow : DateTime.Now;
                    baseEntity.Active = 1;
                }
                else if (t is OracleBaseEntity oracleBaseEntity)
                {
                    oracleBaseEntity.CreateOn = repositoryOption.AutoAddCreateOnUseUtc ? DateTime.UtcNow : DateTime.Now;
                    oracleBaseEntity.Active = 1;
                }
            }

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
            else if (databaseType == DatabaseType.SqlServer || databaseType == DatabaseType.Mysql)
            {
                if (internalResult.IdKeyPropertyInfo != null)
                {
                    var sql = internalResult.Sql + ";" + internalResult.LastInsertIdSql;
                    var dynamicParameters = new DynamicParameters(t);
                    //dynamicParameters.Add("id", null);
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
            var dynamicParameters = ChangeDynamicParameters(internalResult.SqlParameters);
            OpenDb();
            var result = dbConnection.Query<T>(internalResult.Sql, dynamicParameters, dbTransaction).ToList();
            CloseDb();

            return result;
        }

        public int Update(T t)
        {
            if (repositoryOption.AutoUpdateLastUpdateOn)
            {

                if (t is BaseEntity baseEntity)
                {
                    baseEntity.LastUpdateOn = repositoryOption.AutoUpdateLastUpdateOnUseUtc ? DateTime.UtcNow : DateTime.Now;
                }
                else if (t is OracleBaseEntity oracleBaseEntity)
                {
                    oracleBaseEntity.LastUpdateOn = repositoryOption.AutoUpdateLastUpdateOnUseUtc ? DateTime.UtcNow : DateTime.Now;
                }
            }

            var internalResult = InternalUpdate(t);

            OpenDb();
            var result = dbConnection.Execute(internalResult.Sql, t);
            CloseDb();
            return result;
        }

        public int Delete(T t)
        {
            if (t is BaseEntity baseEntity && repositoryOption.IsUseSoftDelete)
            {
                baseEntity.Active = 0;
                return this.Update(t);
            }

            var internalResult = InternalDelete(t);

            OpenDb();
            var result = dbConnection.Execute(internalResult.Sql, t);
            CloseDb();
            return result;
        }

        public int Delete(Expression<Func<T, bool>> predicate)
        {
            var exp = this.Where(predicate).Expression;
            var internalResult = InternalDelete(exp);
            var dynamicParameters = ChangeDynamicParameters(internalResult.SqlParameters);
            OpenDb();
            var result = dbConnection.Execute(internalResult.Sql, dynamicParameters);
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

            if (repositoryOption.AutoAddCreateOn)
            {

                if (t is BaseEntity baseEntity)
                {
                    baseEntity.CreateOn = repositoryOption.AutoAddCreateOnUseUtc ? DateTime.UtcNow : DateTime.Now;
                    baseEntity.Active = 1;
                }
                else if (t is OracleBaseEntity oracleBaseEntity)
                {
                    oracleBaseEntity.CreateOn = repositoryOption.AutoAddCreateOnUseUtc ? DateTime.UtcNow : DateTime.Now;
                    oracleBaseEntity.Active = 1;
                }
            }

            OpenDb();

            if (databaseType == DatabaseType.Oracle)
            {
                var dynamicParameters = new DynamicParameters(t);
                if (internalResult.IdKeyPropertyInfo != null)
                {
                    dynamicParameters.Add(internalResult.IdName, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
                }

                var sql = internalResult.Sql;
                await dbConnection.ExecuteAsync(sql, dynamicParameters, transaction: dbTransaction);

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
            else if (databaseType == DatabaseType.SqlServer || databaseType == DatabaseType.Mysql)
            {
                if (internalResult.IdKeyPropertyInfo != null)
                {
                    var sql = internalResult.Sql + ";" + internalResult.LastInsertIdSql;
                    var dynamicParameters = new DynamicParameters(t);

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
            var dynamicParameters = ChangeDynamicParameters(internalResult.SqlParameters);
            OpenDb();
            var result = (await dbConnection.QueryAsync<T>(internalResult.Sql, dynamicParameters,transaction:dbTransaction)).ToList();
            CloseDb();

            return result;
        }

        public async Task<int> UpdateAsync(T t)
        {
            var internalResult = InternalUpdate(t);
            if (t is BaseEntity baseEntity && repositoryOption.AutoUpdateLastUpdateOn)
            {
                baseEntity.LastUpdateOn = DateTime.Now;
            }

            OpenDb();
            var result = await dbConnection.ExecuteAsync(internalResult.Sql, t);
            CloseDb();
            return result;
        }

        public async Task<int> DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            var exp = this.Where(predicate).Expression;
            var internalResult = InternalDelete(exp);
            var dynamicParameters = ChangeDynamicParameters(internalResult.SqlParameters);
            OpenDb();
            var result = await dbConnection.ExecuteAsync(internalResult.Sql, dynamicParameters);
            CloseDb();
            return result;
        }

        public async Task<int> DeleteAsync(T t)
        {
            if (t is BaseEntity baseEntity && repositoryOption.IsUseSoftDelete)
            {
                baseEntity.Active = 0;
                return await this.UpdateAsync(t);
            }

            var internalResult = InternalDelete(t);

            OpenDb();
            var result = await dbConnection.ExecuteAsync(internalResult.Sql, t);
            CloseDb();
            return result;
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
    }
}