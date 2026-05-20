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
    protected DatabaseUnit databaseUnit;
    protected string parameterPrefix;

    /// <summary>
    /// Left Qualifiers
    /// 左限定符
    /// </summary>
    protected string LeftQualifiers { get; set; }
    /// <summary>
    /// right Qualifiers
    /// 右限定符
    /// </summary>
    protected string RightQualifiers { get; set; }

    public DefaultDatabaseSpecificProvider(IUnitOfWork uow)
    {
        this.uow = uow;
        this.dbFactory = uow.DbFactory;
        this.databaseUnit = dbFactory.DatabaseUnit;
        switch (databaseUnit.DatabaseType)
        {
            case DatabaseType.Pgsql:
            case DatabaseType.Oracle:
                this.parameterPrefix = ":";
                this.LeftQualifiers = "\"";
                this.RightQualifiers = "\"";
                break;
            case DatabaseType.Mysql:
                this.parameterPrefix = "@";
                this.LeftQualifiers = "`";
                this.RightQualifiers = "`";
                break;
            case DatabaseType.SqlServer:
                this.parameterPrefix = "@";
                this.LeftQualifiers = "[";
                this.RightQualifiers = "]";
                break;
            case DatabaseType.Sqlite:
                this.parameterPrefix = ":"; 
                this.LeftQualifiers = "`";
                this.RightQualifiers = "`";
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
    protected string BoxColumnName(string columnName)
    {
        if (columnName == "*")
        {
            return columnName;
        }

        if (databaseUnit.ColumnNameMapping != null)
        {
            columnName = databaseUnit.ColumnNameMapping(columnName);
        }

        return CombineQuoteAndName(columnName);
    }

    protected string BoxTableName(string tableName)
    {
        if (databaseUnit.TableNameMapping != null)
        {
            tableName = databaseUnit.TableNameMapping(tableName);
        }
        return CombineQuoteAndName(tableName);
    }

    private string CombineQuoteAndName(string name)
    {
        return LeftQualifiers + name + RightQualifiers;
    }

    protected string GetSchemaTableName(string schema, string tableName)
    {
        tableName = BoxTableName(tableName);
        tableName = schema.HasText() ? schema + "." + tableName : tableName;
        return tableName;
    }
    public virtual void FastBatchInsert<T>(List<T> list)
    {
        throw new System.NotImplementedException();
    }

    public virtual async Task FastBatchInsertAsync<T>(List<T> list)
    {
        throw new System.NotImplementedException();
    }

    public virtual FastBatchQueryCondition GetBatchQueryCondition<T>(List<T> insertEntities)
    {
        throw new System.NotImplementedException();
    }
}