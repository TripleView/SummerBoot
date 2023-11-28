using SummerBoot.Core;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System;
using SummerBoot.Repository.ExpressionParser.Parser;
using SummerBoot.Repository.Core;
using System.Data.Common;

namespace SummerBoot.Repository
{
    public class CustomBaseRepository<T> : ExpressionParser.Parser.Repository<T>, IBaseRepository<T> where T : class
    {
        public CustomBaseRepository(IUnitOfWork uow, IDbFactory dbFactory)
        {
            this.uow = uow;
            this.dbFactory = dbFactory;
            this.databaseUnit = dbFactory.DatabaseUnit;
            databaseType = databaseUnit.DatabaseType;
            base.Init(databaseUnit);
        }

        protected IUnitOfWork uow;
        protected IDbFactory dbFactory;
        protected IDbConnection dbConnection;
        protected IDbTransaction dbTransaction;
        protected DatabaseType databaseType;
        protected int cmdTimeOut = 1200;
        protected DatabaseUnit databaseUnit;
        public override int InternalExecute(DbQueryResult param)
        {
            databaseUnit.OnLogSqlInfo(param);
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(param.SqlParameters);
            var result = dbConnection.Execute(databaseUnit, param.Sql, dynamicParameters, dbTransaction);
            CloseDb();
            return result;
        }

        public override async Task<int> InternalExecuteAsync(DbQueryResult param)
        {
            databaseUnit.OnLogSqlInfo(param);
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(param.SqlParameters);
            var result = await dbConnection.ExecuteAsync(databaseUnit, param.Sql, dynamicParameters, dbTransaction);
            CloseDb();
            return result;
        }

        public override Page<TResult> InternalQueryPage<TResult>(DbQueryResult param)
        {
            databaseUnit.OnLogSqlInfo(param);
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(param.SqlParameters);

            var count = dbConnection.QueryFirstOrDefault<int>(databaseUnit, param.CountSql, dynamicParameters, dbTransaction);
            var item = dbConnection.Query<TResult>(databaseUnit, param.Sql, dynamicParameters, dbTransaction).ToList();
            CloseDb();
            var result = new Page<TResult>()
            {
                Data = item,
                TotalPages = count
            };
            return result;
        }

        public override async Task<Page<TResult>> InternalQueryPageAsync<TResult>(DbQueryResult param)
        {
            databaseUnit.OnLogSqlInfo(param);
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(param.SqlParameters);

            var count = await dbConnection.QueryFirstOrDefaultAsync<int>(databaseUnit, param.CountSql, dynamicParameters, dbTransaction);
            var item = (await dbConnection.QueryAsync<TResult>(databaseUnit, param.Sql, dynamicParameters, dbTransaction)).ToList();
            CloseDb();
            var result = new Page<TResult>()
            {
                Data = item,
                TotalPages = count
            };
            return result;
        }


        public override List<TResult> InternalQueryList<TResult>(DbQueryResult param)
        {
            databaseUnit.OnLogSqlInfo(param);
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(param.SqlParameters);

            var result = dbConnection.Query<TResult>(databaseUnit, param.Sql, dynamicParameters, dbTransaction).ToList();

            CloseDb();
            return result;
        }

        public override async Task<List<TResult>> InternalQueryListAsync<TResult>(DbQueryResult param)
        {
            databaseUnit.OnLogSqlInfo(param);
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(param.SqlParameters);

            var result = (await dbConnection.QueryAsync<TResult>(databaseUnit, param.Sql, dynamicParameters, dbTransaction)).ToList();

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
            var result = dbConnection.Query<TResult>(databaseUnit, sql, param, dbTransaction).ToList();

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
            var result = (await dbConnection.QueryAsync<TResult>(databaseUnit, sql, param, dbTransaction)).ToList();
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

            var result = dbConnection.QueryFirstOrDefault<TResult>(databaseUnit, sql, param, dbTransaction);

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

            var result = await dbConnection.QueryFirstOrDefaultAsync<TResult>(databaseUnit, sql, param, dbTransaction);

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
            var result = dbConnection.Execute(databaseUnit, sql, param, dbTransaction);
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
            var result = await dbConnection.ExecuteAsync(databaseUnit, sql, param, dbTransaction);
            CloseDb();
            return result;
        }

        public override TResult InternalQuery<TResult>(DbQueryResult param)
        {
            databaseUnit.OnLogSqlInfo(param);
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(param.SqlParameters);

            var result = dbConnection.QueryFirstOrDefault<TResult>(databaseUnit, param.Sql, dynamicParameters, dbTransaction);

            CloseDb();
            return result;

        }

        public override async Task<TResult> InternalQueryAsync<TResult>(DbQueryResult param)
        {
            databaseUnit.OnLogSqlInfo(param);
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(param.SqlParameters);

            var result = await dbConnection.QueryFirstOrDefaultAsync<TResult>(databaseUnit, param.Sql, dynamicParameters, dbTransaction);

            CloseDb();
            return result;
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
            databaseUnit.OnBeforeInsert(t);
            var internalResult = InternalInsert(t);
            databaseUnit.OnLogSqlInfo(internalResult);
            OpenDb();

            if (databaseType == DatabaseType.Oracle || databaseType == DatabaseType.Pgsql)
            {
                var dynamicParameters = new Core.DynamicParameters(t);
                if (internalResult.IdKeyPropertyInfo != null)
                {
                    dynamicParameters.Add(internalResult.IdName, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
                }

                var sql = internalResult.Sql;

                dbConnection.Execute(databaseUnit, sql, dynamicParameters, transaction: dbTransaction);

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

                    var multiResult = dbConnection.QueryMultiple(databaseUnit, sql, dynamicParameters, transaction: dbTransaction);
                    var id = multiResult.Read<int>().FirstOrDefault();

                    if (id != null)
                    {
                        internalResult.IdKeyPropertyInfo.SetValue(t, Convert.ChangeType(id, internalResult.IdKeyPropertyInfo.PropertyType));
                    }
                }
                else
                {
                    dbConnection.Execute(databaseUnit, internalResult.Sql, t, transaction: dbTransaction);
                }
            }
            else if (databaseType == DatabaseType.SqlServer || databaseType == DatabaseType.Mysql)
            {
                if (internalResult.IdKeyPropertyInfo != null)
                {
                    var sql = internalResult.Sql + ";" + internalResult.LastInsertIdSql;
                    var dynamicParameters = new DynamicParameters(t);

                    var multiResult = dbConnection.QueryMultiple(databaseUnit, sql, dynamicParameters, transaction: dbTransaction);
                    var id = multiResult.Read<int>().FirstOrDefault();

                    if (id != null)
                    {
                        internalResult.IdKeyPropertyInfo.SetValue(t, Convert.ChangeType(id, internalResult.IdKeyPropertyInfo.PropertyType));
                    }
                }
                else
                {
                    dbConnection.Execute(databaseUnit, internalResult.Sql, t, transaction: dbTransaction);
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
            databaseUnit.OnLogSqlInfo(internalResult);
            var dynamicParameters = ChangeDynamicParameters(internalResult.SqlParameters);
            OpenDb();
            var result = dbConnection.Query<T>(databaseUnit, internalResult.Sql, dynamicParameters, transaction: dbTransaction).ToList();
            CloseDb();

            return result;
        }

        public int Update(T t)
        {
            //if (databaseUnit.AutoUpdateLastUpdateOn)
            //{

            //    if (t is BaseEntity baseEntity)
            //    {
            //        baseEntity.LastUpdateOn = databaseUnit.AutoUpdateLastUpdateOnUseUtc ? DateTime.UtcNow : DateTime.Now;
            //    }
            //    else if (t is OracleBaseEntity oracleBaseEntity)
            //    {
            //        oracleBaseEntity.LastUpdateOn = databaseUnit.AutoUpdateLastUpdateOnUseUtc ? DateTime.UtcNow : DateTime.Now;
            //    }
            //}
            databaseUnit.OnBeforeUpdate(t);
            var internalResult = InternalUpdate(t);
            databaseUnit.OnLogSqlInfo(internalResult);
            OpenDb();
            var result = dbConnection.Execute(databaseUnit, internalResult.Sql, t, transaction: dbTransaction);
            CloseDb();
            return result;
        }

        public int Delete(T t)
        {
            //if (t is BaseEntity baseEntity && databaseUnit.IsUseSoftDelete)
            //{
            //    baseEntity.Active = 0;
            //    return this.Update(t);
            //}

            var internalResult = InternalDelete(t);
            databaseUnit.OnLogSqlInfo(internalResult);
            OpenDb();
            var result = dbConnection.Execute(databaseUnit, internalResult.Sql, t, transaction: dbTransaction);
            CloseDb();
            return result;
        }

        public int Delete(Expression<Func<T, bool>> predicate)
        {
            var exp = this.Where(predicate).Expression;
            var internalResult = InternalDelete(exp);
            databaseUnit.OnLogSqlInfo(internalResult);
            var dynamicParameters = ChangeDynamicParameters(internalResult.SqlParameters);
            OpenDb();
            var result = dbConnection.Execute(databaseUnit, internalResult.Sql, dynamicParameters, transaction: dbTransaction);
            CloseDb();
            return result;
        }


        public T Get(object id)
        {
            DbQueryResult internalResult = InternalGet(id);
            databaseUnit.OnLogSqlInfo(internalResult);
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(internalResult.SqlParameters);

            var result = dbConnection.QueryFirstOrDefault<T>(databaseUnit, internalResult.Sql, dynamicParameters, transaction: dbTransaction);
            CloseDb();
            return result;
        }

        /// <summary>
        /// 快速批量插入
        /// </summary>
        /// <param name="list"></param>
        public void FastBatchInsert(List<T> list)
        {
            foreach (var t in list)
            {
                databaseUnit.OnBeforeInsert(t);
            }

            var internalResult = InternalFastInsert(list);
            databaseUnit.OnLogSqlInfo(internalResult);
            OpenDb();
            if (databaseUnit.IsOracle)
            {
                var cmd = dbConnection.CreateCommand();
                cmd.CommandText = internalResult.Sql;
                cmd.SetPropertyValue("ArrayBindCount", list.Count);
                if (dbTransaction != null)
                {
                    cmd.Transaction = dbTransaction;
                }

                foreach (var parameter in internalResult.SqlParameters)
                {
                    var param = cmd.CreateParameter();

                    if (parameter.DbType == DbType.Time)
                    {
                        var oracleDbType = param!.GetType()!.GetProperty("OracleDbType")!.PropertyType;
                        var dbtype = Enum.Parse(oracleDbType, "114");
                        param.SetPropertyValue("OracleDbType", dbtype);
                        param.Value = parameter.Value;
                    }
                    else if (parameter.DbType == DbType.Time)
                    {
                        var oracleDbType = param!.GetType()!.GetProperty("OracleDbType")!.PropertyType;
                        var dbtype = Enum.Parse(oracleDbType, "123");
                        param.SetPropertyValue("OracleDbType", dbtype);
                        param.Value = parameter.Value;
                    }
                    else
                    {
                        param.DbType = parameter.DbType;
                        param.Value = parameter.Value;
                    }

                    cmd.Parameters.Add(param);
                }

                var resultCount = cmd.ExecuteNonQuery();
            }
            else if (databaseUnit.IsSqlServer)
            {
                if (SbUtil.CacheDictionary.TryGetValue("sqlBulkCopyDelegate", out var cacheFunc)
                    && SbUtil.CacheDictionary.TryGetValue("sqlBulkCopyDelegate3", out var cacheFunc3)
                    && SbUtil.CacheDictionary.TryGetValue("sqlBulkCopyOptionsType", out var sqlBulkCopyOptionsType))
                {

                    object sqlBulkCopy;
                    if (dbTransaction == null)
                    {
                        sqlBulkCopy = ((Delegate)cacheFunc).DynamicInvoke(this.dbConnection);
                    }
                    else
                    {
                        var dbtype = Enum.Parse((Type)sqlBulkCopyOptionsType, "1");
                        sqlBulkCopy = ((Delegate)cacheFunc3).DynamicInvoke(this.dbConnection, dbtype, dbTransaction);
                    }

                    sqlBulkCopy.SetPropertyValue("BatchSize", 1000);
                    sqlBulkCopy.SetPropertyValue("DestinationTableName", internalResult.Sql);
                    var columnMappings = sqlBulkCopy.GetPropertyValue("ColumnMappings");

                    if (SbUtil.CacheDictionary.TryGetValue("addColumnMappingMethodInfo", out var cacheAddColumnMappingMethodInfo))
                    {
                        var addMethod = ((MethodInfo)cacheAddColumnMappingMethodInfo);
                        foreach (var mapping in internalResult.PropertyInfoMappings)
                        {
                            addMethod.Invoke(columnMappings, parameters: new object[2] { mapping.PropertyInfo.Name, mapping.ColumnName });
                        }
                    }

                    var insertData = list.ToDataTable(internalResult.PropertyInfoMappings.Select(it => it.PropertyInfo).ToList());
                    if (SbUtil.CacheDelegateDictionary.TryGetValue("sqlBulkCopyWriteMethodDelegate",
                            out var sqlBulkCopyWriteMethodAsyncDelegate))
                    {
                        sqlBulkCopyWriteMethodAsyncDelegate.DynamicInvoke(sqlBulkCopy,
                            insertData);
                    }

                }
                else if (SbUtil.CacheDictionary.TryGetValue("sqlBulkCopyDelegateErr", out object cacheException))
                {
                    throw new NotSupportedException("init error", cacheException as Exception);
                }
            }
            else if (databaseUnit.IsMysql)
            {

                if (SbUtil.CacheDictionary.TryGetValue("mysqlBulkCopyType", out var cacheFunc) &&
                    SbUtil.CacheDictionary.TryGetValue("mySqlBulkCopyColumnMappingType", out var mappingType))
                {

                    object mysqlBulkCopy = ((Type)cacheFunc).CreateInstance(new object[2] { dbConnection, dbTransaction });

                    mysqlBulkCopy.SetPropertyValue("DestinationTableName", internalResult.Sql);
                    var columnMappings = mysqlBulkCopy.GetPropertyValue("ColumnMappings");

                    if (SbUtil.CacheDelegateDictionary.TryGetValue("addColumnMappingMethodInfoDelegate",
                            out var cacheAddColumnMappingMethodInfo))
                    {

                        for (int i = 0; i < internalResult.PropertyInfoMappings.Count; i++)
                        {
                            var property = internalResult.PropertyInfoMappings[i].PropertyInfo;
                            var columnName = internalResult.PropertyInfoMappings[i].ColumnName;

                            //continue;
                            if (property.PropertyType.GetUnderlyingType() == typeof(Guid))
                            {
                                var mappingParam = ((Type)mappingType).CreateInstance(new object[3]
                                    { i, "@tmp", $"{columnName} = unhex(@tmp)" });
                                cacheAddColumnMappingMethodInfo.DynamicInvoke(columnMappings, mappingParam);
                            }
                            else
                            {
                                var mappingParam = ((Type)mappingType).CreateInstance(new object[3]
                                    { i, columnName, null });
                                cacheAddColumnMappingMethodInfo.DynamicInvoke(columnMappings, mappingParam);
                            }
                        }
                    }

                    var insertData = list.ToDataTable(internalResult.PropertyInfoMappings.Select(it => it.PropertyInfo).ToList());
                    SbUtil.ReplaceDataTableColumnType<Guid, byte[]>(insertData, guid1 => guid1.ToByteArray());

                    if (SbUtil.CacheDelegateDictionary.TryGetValue("mysqlBulkCopyWriteMethodDelegate", out var mysqlBulkCopyWriteMethodDelete))
                    {
                        var result = mysqlBulkCopyWriteMethodDelete.DynamicInvoke(mysqlBulkCopy, insertData);
                    }

                }
                else if (SbUtil.CacheDictionary.TryGetValue("mysqlBulkCopyDelegateErr", out object cacheException))
                {
                    throw new NotSupportedException("init error", cacheException as Exception);
                }
            }

            CloseDb();
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
            databaseUnit.OnBeforeInsert(t);
            var internalResult = InternalInsert(t);
            databaseUnit.OnLogSqlInfo(internalResult);
            OpenDb();

            if (databaseType == DatabaseType.Oracle || databaseType == DatabaseType.Pgsql)
            {
                var dynamicParameters = new DynamicParameters(t);
                if (internalResult.IdKeyPropertyInfo != null)
                {
                    dynamicParameters.Add(internalResult.IdName, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
                }

                if (databaseType == DatabaseType.Pgsql)
                {
                    foreach (var dynamicParametersGetParamInfo in dynamicParameters.GetParamInfos)
                    {
                        if (dynamicParametersGetParamInfo.Value != null && dynamicParametersGetParamInfo.Value.ValueType != null && (dynamicParametersGetParamInfo.Value.ValueType.IsEnum || (dynamicParametersGetParamInfo.Value.ValueType.IsNullable() && Nullable.GetUnderlyingType(dynamicParametersGetParamInfo.Value.ValueType).IsEnum)))
                        {
                            var enumType = dynamicParametersGetParamInfo.Value.ValueType.IsEnum? dynamicParametersGetParamInfo.Value.ValueType : Nullable.GetUnderlyingType(dynamicParametersGetParamInfo.Value.ValueType);
                            var enumUnderlyingType=  Enum.GetUnderlyingType(enumType);
                            if (dynamicParametersGetParamInfo.Value.Value != null)
                            {
                                if (enumUnderlyingType == typeof(int))
                                {
                                    dynamicParametersGetParamInfo.Value.Value =
                                        Convert.ToInt32(dynamicParametersGetParamInfo.Value.Value);
                                    dynamicParametersGetParamInfo.Value.ValueType = typeof(int);
                                }
                                else if (enumUnderlyingType == typeof(uint))
                                {
                                    dynamicParametersGetParamInfo.Value.Value =
                                        Convert.ToUInt32(dynamicParametersGetParamInfo.Value.Value);
                                    dynamicParametersGetParamInfo.Value.ValueType = typeof(uint);
                                }
                                else if (enumUnderlyingType == typeof(long))
                                {
                                    dynamicParametersGetParamInfo.Value.Value =
                                        Convert.ToInt64(dynamicParametersGetParamInfo.Value.Value);
                                    dynamicParametersGetParamInfo.Value.ValueType = typeof(long);
                                }
                                else if (enumUnderlyingType == typeof(ulong))
                                {
                                    dynamicParametersGetParamInfo.Value.Value =
                                        Convert.ToUInt64(dynamicParametersGetParamInfo.Value.Value);
                                    dynamicParametersGetParamInfo.Value.ValueType = typeof(ulong);
                                }
                                else
                                {
                                    dynamicParametersGetParamInfo.Value.Value =
                                        Convert.ToInt32(dynamicParametersGetParamInfo.Value.Value);
                                    dynamicParametersGetParamInfo.Value.ValueType = typeof(int);
                                }

                            }
                        }
                    }
                }

                var sql = internalResult.Sql;
                await dbConnection.ExecuteAsync(databaseUnit, sql, dynamicParameters, transaction: dbTransaction);

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

                    var multiResult = await dbConnection.QueryMultipleAsync(databaseUnit, sql, dynamicParameters, transaction: dbTransaction);
                    var id = multiResult.Read<int>().FirstOrDefault();


                    if (id != null)
                    {
                        internalResult.IdKeyPropertyInfo.SetValue(t, Convert.ChangeType(id, internalResult.IdKeyPropertyInfo.PropertyType));
                    }
                }
                else
                {
                    await dbConnection.ExecuteAsync(databaseUnit, internalResult.Sql, t, transaction: dbTransaction);
                }
            }
            else if (databaseType == DatabaseType.SqlServer || databaseType == DatabaseType.Mysql)
            {
                if (internalResult.IdKeyPropertyInfo != null)
                {
                    var sql = internalResult.Sql + ";" + internalResult.LastInsertIdSql;
                    var dynamicParameters = new DynamicParameters(t);

                    var multiResult = await dbConnection.QueryMultipleAsync(databaseUnit, sql, dynamicParameters, transaction: dbTransaction);
                    var id = multiResult.Read<int>().FirstOrDefault(); ;

                    if (id != null)
                    {
                        internalResult.IdKeyPropertyInfo.SetValue(t, Convert.ChangeType(id, internalResult.IdKeyPropertyInfo.PropertyType));
                    }
                }
                else
                {
                    await dbConnection.ExecuteAsync(databaseUnit, internalResult.Sql, t, transaction: dbTransaction);
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
            databaseUnit.OnLogSqlInfo(internalResult);
            var dynamicParameters = ChangeDynamicParameters(internalResult.SqlParameters);
            OpenDb();
            var result = (await dbConnection.QueryAsync<T>(databaseUnit, internalResult.Sql, dynamicParameters, transaction: dbTransaction)).ToList();
            CloseDb();

            return result;
        }

        public async Task<int> UpdateAsync(T t)
        {
            //if (databaseUnit.AutoUpdateLastUpdateOn)
            //{

            //    if (t is BaseEntity baseEntity)
            //    {
            //        baseEntity.LastUpdateOn = databaseUnit.AutoUpdateLastUpdateOnUseUtc ? DateTime.UtcNow : DateTime.Now;
            //    }
            //    else if (t is OracleBaseEntity oracleBaseEntity)
            //    {
            //        oracleBaseEntity.LastUpdateOn = databaseUnit.AutoUpdateLastUpdateOnUseUtc ? DateTime.UtcNow : DateTime.Now;
            //    }
            //}
            databaseUnit.OnBeforeUpdate(t);
            var internalResult = InternalUpdate(t);
            databaseUnit.OnLogSqlInfo(internalResult);
            OpenDb();
            var result = await dbConnection.ExecuteAsync(databaseUnit, internalResult.Sql, t, transaction: dbTransaction);
            CloseDb();
            return result;
        }

        public async Task<int> DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            var exp = this.Where(predicate).Expression;
            var internalResult = InternalDelete(exp);
            databaseUnit.OnLogSqlInfo(internalResult);
            var dynamicParameters = ChangeDynamicParameters(internalResult.SqlParameters);
            OpenDb();
            var result = await dbConnection.ExecuteAsync(databaseUnit, internalResult.Sql, dynamicParameters, transaction: dbTransaction);
            CloseDb();
            return result;
        }

        public async Task<int> DeleteAsync(T t)
        {
            //if (t is BaseEntity baseEntity && databaseUnit.IsUseSoftDelete)
            //{
            //    baseEntity.Active = 0;
            //    return await this.UpdateAsync(t);
            //}

            var internalResult = InternalDelete(t);
            databaseUnit.OnLogSqlInfo(internalResult);
            OpenDb();
            var result = await dbConnection.ExecuteAsync(databaseUnit, internalResult.Sql, t, transaction: dbTransaction);
            CloseDb();
            return result;
        }

        public async Task<T> GetAsync(object id)
        {
            DbQueryResult internalResult = InternalGet(id);
            databaseUnit.OnLogSqlInfo(internalResult);
            OpenDb();
            var dynamicParameters = ChangeDynamicParameters(internalResult.SqlParameters);

            var result = await dbConnection.QueryFirstOrDefaultAsync<T>(databaseUnit, internalResult.Sql, dynamicParameters, transaction: dbTransaction);
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
                result.Add(parameter.ParameterName, parameter.Value, valueType: parameter.ParameterType);
            }

            return result;
        }

        public async Task FastBatchInsertAsync(List<T> list)
        {
            foreach (var t in list)
            {
                databaseUnit.OnBeforeInsert(t);
            }

            var internalResult = InternalFastInsert(list);
            databaseUnit.OnLogSqlInfo(internalResult);
            OpenDb();
            if (databaseUnit.IsOracle)
            {
                var cmd = dbConnection.CreateCommand();
                cmd.CommandText = internalResult.Sql;
                cmd.SetPropertyValue("ArrayBindCount", list.Count);
                if (dbTransaction != null)
                {
                    cmd.Transaction = dbTransaction;
                }

                foreach (var parameter in internalResult.SqlParameters)
                {
                    var param = cmd.CreateParameter();

                    if (parameter.DbType == DbType.Time)
                    {
                        var oracleDbType = param!.GetType()!.GetProperty("OracleDbType")!.PropertyType;
                        var dbtype = Enum.Parse(oracleDbType, "114");
                        param.SetPropertyValue("OracleDbType", dbtype);
                        param.Value = parameter.Value;
                    }
                    else if (parameter.DbType == DbType.Time)
                    {
                        var oracleDbType = param!.GetType()!.GetProperty("OracleDbType")!.PropertyType;
                        var dbtype = Enum.Parse(oracleDbType, "123");
                        param.SetPropertyValue("OracleDbType", dbtype);
                        param.Value = parameter.Value;
                    }
                    else
                    {
                        param.DbType = parameter.DbType;
                        param.Value = parameter.Value;
                    }

                    cmd.Parameters.Add(param);
                }

                var resultCount = cmd.ExecuteNonQuery();
            }
            else if (databaseUnit.IsSqlServer)
            {
                if (SbUtil.CacheDictionary.TryGetValue("sqlBulkCopyDelegate", out var cacheFunc)
                    && SbUtil.CacheDictionary.TryGetValue("sqlBulkCopyDelegate3", out var cacheFunc3)
                    && SbUtil.CacheDictionary.TryGetValue("sqlBulkCopyOptionsType", out var sqlBulkCopyOptionsType))
                {
                    object sqlBulkCopy;
                    if (dbTransaction == null)
                    {
                        sqlBulkCopy = ((Delegate)cacheFunc).DynamicInvoke(this.dbConnection);
                    }
                    else
                    {
                        var dbtype = Enum.Parse((Type)sqlBulkCopyOptionsType, "1");
                        sqlBulkCopy = ((Delegate)cacheFunc3).DynamicInvoke(this.dbConnection, dbtype, dbTransaction);
                    }

                    sqlBulkCopy.SetPropertyValue("BatchSize", 1000);
                    sqlBulkCopy.SetPropertyValue("DestinationTableName", internalResult.Sql);
                    var columnMappings = sqlBulkCopy.GetPropertyValue("ColumnMappings");

                    if (SbUtil.CacheDictionary.TryGetValue("addColumnMappingMethodInfo", out var cacheAddColumnMappingMethodInfo))
                    {
                        var addMethod = ((MethodInfo)cacheAddColumnMappingMethodInfo);
                        foreach (var mapping in internalResult.PropertyInfoMappings)
                        {
                            addMethod.Invoke(columnMappings, parameters: new object[2] { mapping.PropertyInfo.Name, mapping.ColumnName });
                        }
                    }

                    var insertData = list.ToDataTable(internalResult.PropertyInfoMappings.Select(it => it.PropertyInfo).ToList());

                    if (SbUtil.CacheDelegateDictionary.TryGetValue("sqlBulkCopyWriteMethodAsyncDelegate",
                            out var sqlBulkCopyWriteMethodAsyncDelegate))
                    {
                        await (Task)sqlBulkCopyWriteMethodAsyncDelegate.DynamicInvoke(sqlBulkCopy,
                            insertData);
                    }
                }
                else if (SbUtil.CacheDictionary.TryGetValue("sqlBulkCopyDelegateErr", out object cacheException))
                {
                    throw new NotSupportedException("init error", cacheException as Exception);
                }
            }
            else if (databaseUnit.IsMysql)
            {

                if (SbUtil.CacheDictionary.TryGetValue("mysqlBulkCopyType", out var cacheFunc) &&
                    SbUtil.CacheDictionary.TryGetValue("mySqlBulkCopyColumnMappingType", out var mappingType))
                {

                    object mysqlBulkCopy = ((Type)cacheFunc).CreateInstance(new object[2] { dbConnection, dbTransaction });

                    mysqlBulkCopy.SetPropertyValue("DestinationTableName", internalResult.Sql);
                    var columnMappings = mysqlBulkCopy.GetPropertyValue("ColumnMappings");
                    var sw = new Stopwatch();

                    if (SbUtil.CacheDelegateDictionary.TryGetValue("addColumnMappingMethodInfoDelegate",
                            out var cacheAddColumnMappingMethodInfo))
                    {

                        for (int i = 0; i < internalResult.PropertyInfoMappings.Count; i++)
                        {
                            var property = internalResult.PropertyInfoMappings[i].PropertyInfo;
                            var columnName = internalResult.PropertyInfoMappings[i].ColumnName;

                            //continue;
                            if (property.PropertyType.GetUnderlyingType() == typeof(Guid))
                            {
                                var mappingParam = ((Type)mappingType).CreateInstance(new object[3]
                                    { i, "@tmp", $"{columnName} = unhex(@tmp)" });
                                cacheAddColumnMappingMethodInfo.DynamicInvoke(columnMappings, mappingParam);
                            }
                            else
                            {
                                var mappingParam = ((Type)mappingType).CreateInstance(new object[3]
                                    { i, columnName, null });
                                cacheAddColumnMappingMethodInfo.DynamicInvoke(columnMappings, mappingParam);
                            }
                        }
                    }

                    var insertData = list.ToDataTable(internalResult.PropertyInfoMappings.Select(it => it.PropertyInfo).ToList());
                    SbUtil.ReplaceDataTableColumnType<Guid, byte[]>(insertData, guid1 => guid1.ToByteArray());

                    if (SbUtil.CacheDelegateDictionary.TryGetValue("mysqlBulkCopyWriteMethodAsyncDelegate", out var mysqlBulkCopyWriteMethodAsyncDelegate))
                    {
                        var result =
                            await (dynamic)mysqlBulkCopyWriteMethodAsyncDelegate.DynamicInvoke(mysqlBulkCopy,
                                insertData, new CancellationToken());
                    }
                }
                else if (SbUtil.CacheDictionary.TryGetValue("mysqlBulkCopyDelegateErr", out object cacheException))
                {
                    throw new NotSupportedException("init error", cacheException as Exception);
                }
            }

            CloseDb();
        }
    }

}
