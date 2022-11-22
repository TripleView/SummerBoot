using System;
using System.Data;
using SummerBoot.Core;

namespace SummerBoot.Repository
{
    /// <summary>
    /// 数据源接口
    /// </summary>
    public interface IDataSource:IDisposable
    {
        IDbConnection GetConnection();
        IDbConnection GetConnection(string connectionString);
    }
}