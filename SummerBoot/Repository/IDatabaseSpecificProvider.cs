using SummerBoot.Core;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace SummerBoot.Repository;

public interface IDatabaseSpecificProvider
{
    /// <summary>
    /// Fast bulk insert
    /// 快速批量插入
    /// </summary>
    /// <param name="list"></param>
    void FastBatchInsert<T>(List<T> list);

    /// <summary>
    /// Fast bulk insert
    /// 快速批量插入
    /// </summary>
    /// <param name="list"></param>
    Task FastBatchInsertAsync<T>(List<T> list);
}

public class DefaultDatabaseSpecificProvider : IDatabaseSpecificProvider
{
    protected IUnitOfWork uow;
    protected IDbConnection dbConnection;
    protected IDbTransaction dbTransaction;
    protected IDbFactory dbFactory;
    public DefaultDatabaseSpecificProvider(IUnitOfWork uow)
    {
        this.uow = uow;
        this.dbFactory = uow.DbFactory;
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

    public virtual void FastBatchInsert<T>(List<T> list)
    {
        throw new System.NotImplementedException();
    }

    public virtual async Task FastBatchInsertAsync<T>(List<T> list)
    {
        throw new System.NotImplementedException();
    }
}