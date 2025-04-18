using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SummerBoot.Core;
using SummerBoot.Repository;

namespace SummerBoot.Repository
{
    /// <summary>
    /// 数据库链接工厂类，负责生成和销毁数据库链接
    /// </summary>
    public class CustomDbFactory : IDbFactory
    {

        public DatabaseUnit DatabaseUnit { get; }

        public CustomDbFactory(DatabaseUnit databaseUnit)
        {
            this.DatabaseUnit = databaseUnit;
        }

        private IDbTransaction shareDbTransaction;
        /// <summary>
        /// 事务所拥有的那个链接，事务提交后，其实链接还在，需要后期手动关闭
        /// </summary>
        private IDbConnection shareDbTransactionLinkDbConnection;
        public void Dispose()
        {
            this.ReleaseDbTransaction();
            this.ReleaseDbConnection();
        }

        public IDbConnection GetDbConnection()
        {
            var dbConnection = (IDbConnection)DatabaseUnit.DbConnectionType.CreateInstance(null);
            dbConnection.ConnectionString = DatabaseUnit.ConnectionString;
            dbConnection.Open();
            return dbConnection;
        }

        public IDbTransaction GetDbTransaction()
        {
            if (shareDbTransaction != null)
            {
                if (shareDbTransaction.Connection != null)
                {
                    return shareDbTransaction;
                }
                shareDbTransaction.Dispose();
                shareDbTransaction = null;
            }

            shareDbTransactionLinkDbConnection ??= this.GetDbConnection();
            if (shareDbTransactionLinkDbConnection.State == ConnectionState.Closed)
            {
                shareDbTransactionLinkDbConnection.Open();
            }
            
            shareDbTransaction = shareDbTransactionLinkDbConnection.BeginTransaction();

            return shareDbTransaction;
        }

        /// <summary>
        /// 释放事务对象
        /// </summary>
        private void ReleaseDbTransaction()
        {
            shareDbTransaction?.Dispose();
            shareDbTransaction = null;
        }

        /// <summary>
        /// 释放共有链接
        /// </summary>
        private void ReleaseDbConnection()
        {
            shareDbTransactionLinkDbConnection?.Close();
            shareDbTransactionLinkDbConnection = null;
        }

        public void ReleaseResources()
        {
            ReleaseDbTransaction();
            ReleaseDbConnection();
        }
    }
}