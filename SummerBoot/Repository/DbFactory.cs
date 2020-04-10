using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.IO;
using SummerBoot.Core;
using SummerBoot.Repository;

namespace SummerBoot.Repository
{
    /// <summary>
    /// 数据库链接工厂类，负责生成和销毁数据库链接
    /// </summary>
    public class DbFactory : IDbFactory
    {
        //private readonly IDataSource dataSource;
        private readonly RepositoryOption repositoryOption;

        public DbFactory( RepositoryOption option)
        {
            this.repositoryOption = option;
        }
        /// <summary>
        /// 长连接
        /// </summary>
        public IDbConnection LongDbConnection { private set; get; }

        /// <summary>
        /// 长连接的事物
        /// </summary>
        public IDbTransaction LongDbTransaction { private set; get; }

        /// <summary>
        /// 短链接
        /// </summary>
        public IDbConnection ShortDbConnection
        {
            get
            {
                //var dbConnection=dataSource.GetConnection();
                var dbConnection =(IDbConnection) repositoryOption.DbConnectionType.CreateInstance(null);
                dbConnection.ConnectionString = repositoryOption.ConnectionString;
                dbConnection.Open();
                return dbConnection;
            }
        }

        /// <summary>
        /// 开启事务
        /// </summary>
        /// <returns></returns>
        public void BeginTransaction()
        {
            if (LongDbConnection == null)
            {
                //LongDbConnection = dataSource.GetConnection();
                LongDbConnection = (IDbConnection)repositoryOption.DbConnectionType.CreateInstance(null);
                LongDbConnection.ConnectionString = repositoryOption.ConnectionString;
                LongDbConnection.Open();
                LongDbTransaction = LongDbConnection.BeginTransaction();
            }
        }

        public void Dispose()
        {
            LongDbTransaction?.Dispose();
            if (LongDbConnection?.State != ConnectionState.Closed)
            {
                LongDbConnection?.Close();
            }
            LongDbConnection?.Dispose();
            LongDbTransaction = null;
            LongDbConnection = null;
        }
    }
}