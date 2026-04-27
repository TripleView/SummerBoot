using SqlParser.Net;
using SummerBoot.Core;
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
using DbType = SqlParser.Net.DbType;

namespace SummerBoot.Repository;

public class NewRepository<T> : OrderLambdaRepository<T>, INewRepository<T>
{
    public NewRepository(IUnitOfWork uow)
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
        Provider = new RepositoryProvider<T>(databaseUnit, this);
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

    #region sync

    public T Get(object id)
    {
        throw new NotImplementedException();
    }

    public List<T> GetAll()
    {
        throw new NotImplementedException();
    }

    public int Update(T t)
    {
        throw new NotImplementedException();
    }

    public void Update(List<T> list)
    {
        throw new NotImplementedException();
    }

    public int Delete(T t)
    {
        throw new NotImplementedException();
    }

    public void Delete(List<T> list)
    {
        throw new NotImplementedException();
    }

    public int Delete(Expression<Func<T, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public T Insert(T t)
    {
        throw new NotImplementedException();
    }

    public List<T> Insert(List<T> list)
    {
        throw new NotImplementedException();
    }

    public void FastBatchInsert(List<T> list)
    {
        throw new NotImplementedException();
    }

    #endregion sync

    #region async

    public async Task<T> GetAsync(object id)
    {
        await Task.Yield();
        throw new NotImplementedException();
    }

    public async Task<List<T>> GetAllAsync()
    {
        await Task.Yield();
        throw new NotImplementedException();
    }

    public async Task<int> UpdateAsync(T t)
    {
        await Task.Yield();
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(List<T> list)
    {
        await Task.Yield();
        throw new NotImplementedException();
    }

    public async Task<int> DeleteAsync(T t)
    {
        await Task.Yield();
        throw new NotImplementedException();
    }

    public async Task<int> DeleteAsync(Expression<Func<T, bool>> predicate)
    {
        await Task.Yield();
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(List<T> list)
    {
        await Task.Yield();
        throw new NotImplementedException();
    }

    public async Task<T> InsertAsync(T t)
    {
        await Task.Yield();
        throw new NotImplementedException();
    }

    public async Task<List<T>> InsertAsync(List<T> list)
    {
        await Task.Yield();
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public TResult QueryFirstOrDefault<TResult>(string sql, object param = null)
    {
        throw new NotImplementedException();
    }

    public int Execute(string sql, object param = null)
    {
        throw new NotImplementedException();
    }

    public Page<TResult> QueryPage<TResult>(string sql, Pageable pageParameter, object param = null)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region async

    public async Task<List<TResult>> QueryListAsync<TResult>(string sql, object param = null)
    {
        await Task.Yield();
        throw new NotImplementedException();
    }

    public async Task<TResult> QueryFirstOrDefaultAsync<TResult>(string sql, object param = null)
    {
        await Task.Yield();
        throw new NotImplementedException();
    }

    public async Task<int> ExecuteAsync(string sql, object param = null)
    {
        await Task.Yield();
        throw new NotImplementedException();
    }

    public async Task<Page<TResult>> QueryPageAsync<TResult>(string sql, Pageable pageParameter, object param = null)
    {
        await Task.Yield();
        throw new NotImplementedException();
    }

    #endregion
}