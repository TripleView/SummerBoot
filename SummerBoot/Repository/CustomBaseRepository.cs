using SummerBoot.Core;
using SummerBoot.Repository.Core;
using SummerBoot.Repository.ExpressionParser;
using SummerBoot.Repository.ExpressionParser.Parser;
using SummerBoot.Repository.ExpressionParser.Parser.Dialect;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DbType = SqlParser.Net.DbType;

namespace SummerBoot.Repository;

public class CustomBaseRepository<T> : PageLambdaRepository<T>, IBaseRepository<T>
{
    public CustomBaseRepository(IUnitOfWork uow, IDatabaseSpecificProvider databaseSpecificProvider) : base(null, null)
    {
        this.uow = uow;
        this.dbFactory = uow.DbFactory;
        this.databaseUnit = dbFactory.DatabaseUnit;
        this.databaseType = databaseUnit.DatabaseType;
        this.entityClassHandler = uow.EntityClassHandler;
        this.databaseSpecificProvider = databaseSpecificProvider;
        Init(databaseUnit);
    }

    protected IUnitOfWork uow;
    protected IDbFactory dbFactory;
    protected IDbConnection dbConnection;
    protected IDbTransaction dbTransaction;
    protected DatabaseType databaseType;
    protected int cmdTimeOut = 1200;
    protected DatabaseUnit databaseUnit;
    protected IEntityClassHandler entityClassHandler;
    protected IDatabaseSpecificProvider databaseSpecificProvider;
    private DbType dbType;
    protected bool IsOracle => this.dbType == DbType.Oracle;
    protected bool IsSqlServer => this.dbType == DbType.SqlServer;
    protected bool IsPgsql => this.dbType == DbType.Pgsql;

    protected bool IsMySql => this.dbType == DbType.MySql;
    protected bool IsSqlite => this.dbType == DbType.Sqlite;

    private QueryFormatter QueryFormatter;
    protected void Init(DatabaseUnit databaseUnit)
    {
        Provider = new RepositoryProvider(databaseUnit, this);
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


    protected DbQueryResult InternalGet(object id)
    {
        var result = new NewDbExpressionVisitor(databaseUnit).Get<T>(id);
        return result;
    }
    #region sync

    public T Get(object id)
    {
        DbQueryResult internalResult = InternalGet(id);
        databaseUnit.OnLogSqlInfo(internalResult);
        OpenDb();
        var result = dbConnection.QueryFirstOrDefault<T>(databaseUnit, internalResult.Sql, internalResult.DynamicParameters, transaction: dbTransaction);
        CloseDb();
        return result;
    }

    public List<T> GetAll()
    {
        var internalResult = InternalGetAll();
        databaseUnit.OnLogSqlInfo(internalResult);
        OpenDb();
        var result = dbConnection.Query<T>(databaseUnit, internalResult.Sql, internalResult.DynamicParameters, transaction: dbTransaction).ToList();
        CloseDb();

        return result;
    }
    protected DbQueryResult InternalUpdate(T updateEntity)
    {
        var result = new NewDbExpressionVisitor(databaseUnit).Update(updateEntity);
        return result;
    }
    public int Update(T t)
    {
        if (t is IBaseEntity baseEntity)
        {
            entityClassHandler.ProcessingEntity(baseEntity, true);
        }
        databaseUnit.OnBeforeUpdate(t);
        var internalResult = InternalUpdate(t);
        databaseUnit.OnLogSqlInfo(internalResult);
        OpenDb();
        var result = dbConnection.Execute(databaseUnit, internalResult.Sql, t, transaction: dbTransaction);
        CloseDb();
        return result;
    }

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

    public int Delete(T t)
    {
        var internalResult = InternalDelete(t);
        databaseUnit.OnLogSqlInfo(internalResult);
        OpenDb();
        var result = dbConnection.Execute(databaseUnit, internalResult.Sql, t, transaction: dbTransaction);
        CloseDb();
        return result;
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

    public int Delete(Expression<Func<T, bool>> predicate)
    {
        var methodInfo = RepositoryMethodsCache.DeleteWithPredicate.MakeGenericMethod(typeof(T));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Expression,
            Expression.Quote(predicate)
        );

        var result = Provider.Execute(callExpr);
        return result;
    }

    protected DbQueryResult InternalInsert(T insertEntity)
    {
        var result = new NewDbExpressionVisitor(databaseUnit).Insert(insertEntity);
        return result;
    }

    public T Insert(T t)
    {
        databaseUnit.OnBeforeInsert(t);
        if (t is IBaseEntity baseEntity)
        {
            entityClassHandler.ProcessingEntity(baseEntity);
        }

        var internalResult = InternalInsert(t);
        databaseUnit.OnLogSqlInfo(internalResult);
        OpenDb();

        if (databaseType == DatabaseType.Oracle || databaseType == DatabaseType.Pgsql)
        {
            var dynamicParameters = new Core.DynamicParameters(t);
            if (internalResult.IdKeyPropertyInfo != null)
            {
                dynamicParameters.Add(internalResult.IdName, 0, dbType: System.Data.DbType.Int32, direction: ParameterDirection.Output);
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

    public void FastBatchInsert(List<T> list)
    {
        if (list.Count == 0)
        {
            return;
        }
        foreach (var t in list)
        {
            databaseUnit.OnBeforeInsert(t);
            if (t is IBaseEntity baseEntity)
            {
                entityClassHandler.ProcessingEntity(baseEntity);
            }
        }

        var internalResult = InternalFastInsert(list);
        //databaseUnit.OnLogSqlInfo(internalResult);
        OpenDb();
        if (databaseUnit.IsOracle)
        {
            uow.BeginTransaction();
            OpenDb();
            list.BatchExecution((tempList) =>
                OracleFastBatchInsert(tempList), 100000);
            uow.Commit();
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
        else
        {
            CloseDb();
            throw new NotSupportedException("not support dbtype:" + this);
        }
        CloseDb();
    }

    #endregion sync

    #region async

    public async Task<T> GetAsync(object id)
    {
        DbQueryResult internalResult = InternalGet(id);
        databaseUnit.OnLogSqlInfo(internalResult);
        OpenDb();
        var result = await dbConnection.QueryFirstOrDefaultAsync<T>(databaseUnit, internalResult.Sql, internalResult.DynamicParameters, transaction: dbTransaction);
        CloseDb();
        return result;
    }
    protected DbQueryResult InternalGetAll()
    {
        var result = new NewDbExpressionVisitor(databaseUnit).GetAll<T>();
        return result;
    }
    public async Task<List<T>> GetAllAsync()
    {
        var internalResult = InternalGetAll();
        databaseUnit.OnLogSqlInfo(internalResult);
        OpenDb();
        var result = (await dbConnection.QueryAsync<T>(databaseUnit, internalResult.Sql, internalResult.DynamicParameters, transaction: dbTransaction)).ToList();
        CloseDb();

        return result;
    }

    public async Task<int> UpdateAsync(T t)
    {
        if (t is IBaseEntity baseEntity)
        {
            await entityClassHandler.ProcessingEntityAsync(baseEntity, true);
        }
        databaseUnit.OnBeforeUpdate(t);
        var internalResult = InternalUpdate(t);
        databaseUnit.OnLogSqlInfo(internalResult);
        OpenDb();
        var result = await dbConnection.ExecuteAsync(databaseUnit, internalResult.Sql, t, transaction: dbTransaction);
        CloseDb();
        return result;
    }

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

    protected DbQueryResult InternalDelete(T deleteEntity)
    {
        var result = new NewDbExpressionVisitor(databaseUnit).Delete(deleteEntity);
        return result;
    }

    public async Task<int> DeleteAsync(T t)
    {
        var internalResult = InternalDelete(t);
        databaseUnit.OnLogSqlInfo(internalResult);
        OpenDb();
        var result = await dbConnection.ExecuteAsync(databaseUnit, internalResult.Sql, internalResult.DynamicParameters, transaction: dbTransaction);
        CloseDb();
        return result;
    }

    public async Task<int> DeleteAsync(Expression<Func<T, bool>> predicate)
    {
        var methodInfo = RepositoryMethodsCache.DeleteWithPredicate.MakeGenericMethod(typeof(T));
        var callExpr = Expression.Call(
            null,
            methodInfo,
            Expression,
            Expression.Quote(predicate)
        );

        var result = await Provider.ExecuteAsync(callExpr);
        return result;
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
        if (t is IBaseEntity baseEntity)
        {
            await entityClassHandler.ProcessingEntityAsync(baseEntity);
        }
        databaseUnit.OnBeforeInsert(t);
        var internalResult = InternalInsert(t);
        databaseUnit.OnLogSqlInfo(internalResult);
        OpenDb();

        if (databaseType == DatabaseType.Oracle || databaseType == DatabaseType.Pgsql)
        {
            var dynamicParameters = new DynamicParameters(t);
            if (internalResult.IdKeyPropertyInfo != null && !databaseUnit.IsDataMigrateMode)
            {
                dynamicParameters.Add(internalResult.IdName, 0, dbType: System.Data.DbType.Int32, direction: ParameterDirection.Output);
            }

            if (databaseType == DatabaseType.Pgsql)
            {
                foreach (var dynamicParametersGetParamInfo in dynamicParameters.GetParamInfos)
                {
                    if (dynamicParametersGetParamInfo.Value != null && dynamicParametersGetParamInfo.Value.ValueType != null && (dynamicParametersGetParamInfo.Value.ValueType.IsEnum || (dynamicParametersGetParamInfo.Value.ValueType.IsNullable() && Nullable.GetUnderlyingType(dynamicParametersGetParamInfo.Value.ValueType).IsEnum)))
                    {
                        var enumType = dynamicParametersGetParamInfo.Value.ValueType.IsEnum ? dynamicParametersGetParamInfo.Value.ValueType : Nullable.GetUnderlyingType(dynamicParametersGetParamInfo.Value.ValueType);
                        var enumUnderlyingType = Enum.GetUnderlyingType(enumType);
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
        else if (databaseType == DatabaseType.SqlServer || databaseType == DatabaseType.Mysql || databaseType == DatabaseType.Sqlite)
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

    public async Task FastBatchInsertAsync(List<T> list)
    {
        if (list.Count == 0)
        {
            return;
        }
        foreach (var t in list)
        {
            if (t is IBaseEntity baseEntity)
            {
                await entityClassHandler.ProcessingEntityAsync(baseEntity);
            }
            databaseUnit.OnBeforeInsert(t);
        }

        var internalResult = InternalFastInsert(list);
        //databaseUnit.OnLogSqlInfo(internalResult);
        OpenDb();
        await databaseSpecificProvider.FastBatchInsertAsync(list); 
        CloseDb();
    }

    #endregion async

    /// <summary>
    /// 快速批量插入
    /// </summary>
    /// <param name="insertEntities"></param>
    /// <returns></returns>
    protected FastBatchQueryCondition InternalFastInsert(List<T> insertEntities)
    {
        //if (IsSqlServer)
        //{
        //    var table = SbUtil.GetTableInfo(typeof(T));
        //    var tableName = GetSchemaTableName(table.Schema, table.Name);

        //    var result = new DbQueryResult()
        //    {
        //        Sql = tableName,
        //        DynamicParameters = this.dynamicParameters,
        //        PropertyInfoMappings = table.Columns.Where(it => !(it.IsKey && it.IsDatabaseGeneratedIdentity)).Select(it => new DbQueryResultPropertyInfoMapping() { ColumnName = it.Name, PropertyInfo = it.Property }).ToList()
        //    };
        //}
        return QueryFormatter.FastBatchInsert(insertEntities);
    }
    private async void OracleFastBatchInsert(List<T> list)
    {
        var internalResult = InternalFastInsert(list);
        var cmd = dbConnection.CreateCommand();
        //cmd.CommandTimeout = 100000000;
        cmd.CommandText = internalResult.Sql;
        cmd.SetPropertyValue("ArrayBindCount", list.Count);
        if (dbTransaction != null)
        {
            cmd.Transaction = dbTransaction;
        }

        foreach (var parameter in internalResult.FastBatchSqlParameters)
        {
            var param = cmd.CreateParameter();
            if (parameter.DbType == System.Data.DbType.Time)
            {
                var oracleDbType = param!.GetType()!.GetProperty("OracleDbType")!.PropertyType;
                var dbtype = Enum.Parse(oracleDbType, "114");
                param.SetPropertyValue("OracleDbType", dbtype);
                param.Value = parameter.Value;
            }
            else if (parameter.DbType == System.Data.DbType.Time)
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

    #region sync

    public List<TResult> QueryList<TResult>(string sql, object param = null)
    {
        OpenDb();
        var result = dbConnection.Query<TResult>(databaseUnit, sql, param, dbTransaction).ToList();
        CloseDb();
        return result;
    }

    public TResult QueryFirstOrDefault<TResult>(string sql, object param = null)
    {
        OpenDb();
        var result = dbConnection.QueryFirstOrDefault<TResult>(databaseUnit, sql, param, dbTransaction);
        CloseDb();
        return result;
    }

    public int Execute(string sql, object param = null)
    {
        OpenDb();
        var result = dbConnection.Execute(databaseUnit, sql, param, dbTransaction);
        CloseDb();
        return result;
    }

    public Page<TResult> QueryPage<TResult>(string sql, Pageable pageParameter, object param = null)
    {
        throw new NotImplementedException();
    }
    public Page<TResult> QueryPageWithFullSql<TResult>(string pageSql, string countSql, object param = null)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region async

    public async Task<List<TResult>> QueryListAsync<TResult>(string sql, object param = null)
    {
        OpenDb();
        var result = (await dbConnection.QueryAsync<TResult>(databaseUnit, sql, param, dbTransaction)).ToList();
        CloseDb();
        return result;
    }

    public async Task<TResult> QueryFirstOrDefaultAsync<TResult>(string sql, object param = null)
    {
        OpenDb();
        var result = await dbConnection.QueryFirstOrDefaultAsync<TResult>(databaseUnit, sql, param, dbTransaction);
        CloseDb();
        return result;
    }

    public async Task<int> ExecuteAsync(string sql, object param = null)
    {
        OpenDb();
        var result = await dbConnection.ExecuteAsync(databaseUnit, sql, param, dbTransaction);
        CloseDb();
        return result;
    }

    public async Task<Page<TResult>> QueryPageAsync<TResult>(string sql, Pageable pageParameter, object param = null)
    {
        OpenDb();
        var count = await dbConnection.QueryFirstOrDefaultAsync<int>(databaseUnit, "", param, dbTransaction);
        var item = (await dbConnection.QueryAsync<TResult>(databaseUnit, sql, param, dbTransaction)).ToList();
        CloseDb();
        var result = new Page<TResult>()
        {
            Data = item,
            TotalPages = count
        };
        return result;
    }
    public async Task<Page<TResult>> QueryPageWithFullSqlAsync<TResult>(string pageSql, string countSql, object param = null)
    {
        OpenDb();
        var count = await dbConnection.QueryFirstOrDefaultAsync<int>(databaseUnit, countSql, param, dbTransaction);
        var item = (await dbConnection.QueryAsync<TResult>(databaseUnit, pageSql, param, dbTransaction)).ToList();
        CloseDb();
        var result = new Page<TResult>()
        {
            Data = item,
            TotalPages = count
        };
        return result;
    }
    #endregion
}