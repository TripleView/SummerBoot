using SqlParser.Net;
using SummerBoot.Core;
using SummerBoot.Repository.Core;
using SummerBoot.Repository.ExpressionParser.Parser;
using SummerBoot.Repository.ExpressionParser.Parser.Dialect;
using SummerBoot.Repository.MultiQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using SummerBoot.Repository.ExpressionParser;
using DbType = SqlParser.Net.DbType;

namespace SummerBoot.Repository;

public class CustomBaseRepository<T> : OrderLambdaRepository<T>, IBaseRepository<T>
{
    public CustomBaseRepository(IUnitOfWork uow) : base(null, null)
    {
        this.uow = uow;
        this.dbFactory = uow.DbFactory;
        this.databaseUnit = dbFactory.DatabaseUnit;
        this.databaseType = databaseUnit.DatabaseType;
        this.entityClassHandler = uow.EntityClassHandler;
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
    private DbType dbType;

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
        var exp = this.Where(predicate).Expression;
        var methodInfo = QueryableMethodsCache.ExecuteDelete;
        var callExpr = Expression.Call(
            null,
            methodInfo,
            exp
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
        throw new NotImplementedException();
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
        await Task.Yield();
        throw new NotImplementedException();
    }

    #endregion async

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
        await Task.Yield();
        throw new NotImplementedException();
    }

    #endregion
}