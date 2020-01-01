using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using SummerBoot.Core;

namespace SummerBoot.Repository.Druid
{
    public sealed class DruidConnectionHolder
    {
        private static readonly ILogger logger=new LoggerFactory().CreateLogger(typeof(DruidConnectionHolder).Name);
        /// <summary>
        /// 数据源
        /// </summary>
        public DruidDataSource DataSource { get; }
        /// <summary>
        /// 数据库连接Id
        /// </summary>
        public long ConnectionId { get; }
        /// <summary>
        /// 数据库连接
        /// </summary>
        public IDbConnection Connection { get; }
        /// <summary>
        /// 连接毫秒数
        /// </summary>
        public long ConnectTimeMillis { get; }

        public long CreateNanoSpan { get; }
        /// <summary>
        /// 最后一次激活的毫秒数
        /// </summary>
        public long LastActiveTimeMillis { set; get; }
        /// <summary>
        /// 最后一次保持连接的毫秒数
        /// </summary>
        protected readonly long lastKeepTimeMillis;
        /// <summary>
        /// 最后一次有效的毫秒数
        /// </summary>
        protected readonly long lastValidTimeMillis;
        /// <summary>
        /// 最后一次校验时间
        /// </summary>
        public long LastValidTimeMillis { set; get; }

        public bool Discard { set; get; } = false;

        public long LastKeepTimeMillis { set; get; }

        public long KeepAliveCheckCount { get; private set; } = 0;
      
        public long LastNotEmptyWaitNanos { set; get; }
        /// <summary>
        /// 使用次数
        /// </summary>
        public long UseCount { set; get; } = 0;

        public DruidConnectionHolder(DruidDataSource dataSource,IDbConnection dbConnection, long connectNanoSpan)
        {
            Trace.Assert(dataSource != null, "dataSource can not be null");

            this.DataSource = dataSource;
            this.Connection = dbConnection;
            this.CreateNanoSpan = connectNanoSpan;
            this.ConnectTimeMillis = SbUtil.CurrentTimeMillis();
            this.LastActiveTimeMillis = ConnectTimeMillis;
            this.ConnectionId = dataSource.CreateConnectionId();
        }

        public void IncrementUseCount()
        {
            UseCount++;
        }

        public DruidConnectionHolder(DruidDataSource dataSource, PhysicalConnectionInfo pyConnectInfo):this(dataSource,pyConnectInfo.DbConnection,pyConnectInfo.ConnectNanoSpan)
        {
        }

        public void IncrementKeepAliveCheckCount()
        {
            KeepAliveCheckCount++;
        }

        public void Reset()
        {

        }
    }
}