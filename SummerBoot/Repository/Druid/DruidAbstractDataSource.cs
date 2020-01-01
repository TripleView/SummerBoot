using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using SummerBoot.Core;

namespace SummerBoot.Repository.Druid
{
    public abstract class DruidAbstractDataSource : IDataSource
    {
        public static readonly int DEFAULT_TIME_BETWEEN_EVICTION_RUNS_MILLIS = 60 * 1000;
        /// <summary>
        /// 默认最大闲散数
        /// </summary>
        public readonly static int DEFAULT_MAX_IDLE = 8;
        /// <summary>
        /// 默认最小闲散数
        /// </summary>
        public readonly static int DEFAULT_MIN_IDLE = 0;
        public readonly static int DEFAULT_MAX_WAIT = -1;
        /// <summary>
        /// 获取连接时最大等待时间，单位毫秒
        /// </summary>
        protected long maxWait = DEFAULT_MAX_WAIT;
        public static readonly long DEFAULT_MIN_EVICTABLE_IDLE_TIME_MILLIS = 1000L * 60L * 30L;
        public static readonly long DEFAULT_MAX_EVICTABLE_IDLE_TIME_MILLIS = 1000L * 60L * 60L * 7;
        protected volatile bool initExceptionThrow = true;
        protected DateTime initedTime;
        /// <summary>
        /// 判断链接是否有效的校验器
        /// </summary>
        public IValidConnectionChecker ValidConnectionChecker { set; get; }

        private Timer destroyTimer;

        public Timer DestroyTimer
        {
            get => destroyTimer;
            set
            {
                if(Inited)throw new Exception("dataSource inited");
                destroyTimer = value;
            }
        }
        private Timer createTimer;

        public Timer CreateTimer
        {
            get => createTimer;
            set
            {
                if (Inited) throw new Exception("dataSource inited");
                createTimer = value;
            }
        }
        /// <summary>
        /// 创建链接失败后中断标志
        /// </summary>
        protected bool breakAfterAcquireFailure = false;
        /// <summary>
        /// 用来检测连接是否有效的sql，要求是一个查询语句。
        ///如果validationQuery为null，testOnBorrow、testOnReturn、
        ///testWhileIdle都不会其作用。在mysql中通常为select 'x'，在oracle中通常为 select 1 from dual
        /// </summary>
        public string ValidationQuery { set; get; } = "";
        /// <summary>
        /// 验证链接是否有效的超时时间
        /// </summary>
        public int ValidationQueryTimeout { set; get; } = -1;
        /// <summary>
        /// dataSource是否初始化的标志
        /// </summary>
        protected volatile bool Inited = false;

        /// <summary>
        /// connectionId起始种子
        /// </summary>
        protected volatile int connectionIdSeed = 10000;

        /// <summary>
        /// metaDataId起始种子
        /// </summary>
        protected volatile int metaDataIdSeed = 80000;
        /// <summary>
        /// 开始创建链接的时间
        /// </summary>
        protected long createStartNanos = 0;
        /// <summary>
        /// 允许的空闲时间的上限值，当一个idbConnection空闲了太久，就可能被移除
        /// </summary>
        protected long maxEvictableIdleTimeMillis = DEFAULT_MAX_EVICTABLE_IDLE_TIME_MILLIS;
        /// <summary>
        /// 创建链接错误次数
        /// </summary>
        protected volatile int createErrorCount = 0;

        /// <summary>
        /// 对于建立时间超过removeAbandonedTimeout的连接强制移除
        /// </summary>
        protected bool removeAbandoned;

        protected static readonly object PRESENT = new object();
        protected volatile int maxWaitThreadCount = -1;
        protected volatile int creatingCount = 0;
        /// <summary>
        /// 直接创建链接计数
        /// </summary>
        protected volatile int directCreateCount = 0;
        protected volatile int onFatalErrorMaxActive = 0;
        /// <summary>
        /// 物理连接的最大使用次数
        /// </summary>
        protected volatile int phyMaxUseCount = -1;
        protected volatile Exception lastFatalError = null;
        /// <summary>
        /// 创建时发生的错误
        /// </summary>
        protected volatile Exception createError = null;
        protected volatile Exception disableException = null;
        protected  long failContinuousTimeMillis = 0L;
        /// <summary>
        /// 归还连接时执行validationQuery检测连接是否有效，
        ///做了这个配置会降低性能
        /// </summary>
        protected volatile bool testOnReturn = false;
        /// <summary>
        /// 建议配置为true，不影响性能，并且保证安全性。
        ///申请连接的时候检测，如果空闲时间大于
        ///timeBetweenEvictionRunsMillis，
        ///执行validationQuery检测连接是否有效。
        /// </summary>
        protected bool testWhileIdle = true;

        protected bool failFast = false;
        protected volatile int failContinuous = 0;

        protected readonly IDictionary<DruidDbConnection,object> activeConnections=new Dictionary<DruidDbConnection, object>();

        protected volatile bool logAbandoned;
        /// <summary>
        /// 指定连接建立多长时间就需要被强制关闭
        /// </summary>
        protected long removeAbandonedTimeoutMillis = 300 * 1000;
        /// <summary>
        /// 池中的允许的最小空闲数量
        /// </summary>
        protected volatile int minIdle = DEFAULT_MIN_IDLE;
        /// <summary>
        /// IdbConnection连接到数据库的时间与当前时间的最大时间差,超过这个时间差将移除这个IdbConnection
        /// </summary>
        protected long phyTimeoutMillis = DEFAULT_PHY_TIMEOUT_MILLIS;
        /// <summary>
        /// 允许的数据库链接的存活时间
        /// </summary>
        protected long keepAliveBetweenTimeMillis = DEFAULT_TIME_BETWEEN_EVICTION_RUNS_MILLIS * 2;
        /// <summary>
        /// 创建链接计数
        /// </summary>
        protected long createCount = 0;

        /// <summary>
        /// 池中的连接数量
        /// </summary>
        protected int poolingCount = 0;
        /// <summary>
        /// 失效数量
        /// </summary>
        protected long discardCount = 0;
        protected int createTaskCount;
        /// <summary>
        /// 线程池峰值
        /// </summary>
        protected int poolingPeak = 0;
        protected int keepAliveCheckCount = 0;
        /// <summary>
        /// 致命的错误数量
        /// </summary>
        protected volatile int fatalErrorCount = 0;
        /// <summary>
        /// 线程池峰值发生时间
        /// </summary>
        protected long poolingPeakTime = 0;
        /// <summary>
        /// 上次致命错误发生时间
        /// </summary>
        protected long lastFatalErrorTimeMillis = 0;

        /// <summary>
        /// 如果检测到当前连接的最后活跃时间和当前时间的差值大于
        /// minEvictableIdleTimeMillis，则关闭当前连接。
        /// </summary>
        protected long minEvictableIdleTimeMillis = DEFAULT_MIN_EVICTABLE_IDLE_TIME_MILLIS;

        protected long notEmptySignalCount = 0L;

        /// <summary>
        /// 销毁计数
        /// </summary>
        protected int destroyCount = 0;
        /// <summary>
        /// 活跃的数量，就是被拿去用了
        /// </summary>
        protected int activeCount = 0;
        /// <summary>
        /// 每次检查强制验证连接有效性
        /// </summary>
        protected volatile bool keepAlive = false;



        /// <summary>
        /// 连接池是否已关闭
        /// </summary>
        protected volatile bool closed = false;

        /// <summary>
        /// 是否发生致命错误
        /// </summary>
        protected volatile bool onFatalError = false;

        /// <summary>
        /// 上次收缩时的致命错误数量
        /// </summary>
        protected volatile int fatalErrorCountLastShrink = 0;


        public long CreateTimespan { get; set; }
        /// <summary>
        /// 最大活跃数量
        /// </summary>
        protected volatile int maxActive = DEFAULT_MAX_ACTIVE_SIZE;
        /// <summary>
        /// 在空闲连接回收器线程运行期间休眠的时间值,以毫秒为单位.
        ///如果设置为非正数,则不运行空闲连接回收器线程
        /// </summary>
        protected volatile int timeBetweenEvictionRunsMillis = DEFAULT_TIME_BETWEEN_EVICTION_RUNS_MILLIS;
        /// <summary>
        /// 创建链接失败后到继续重试的时间间隔
        /// </summary>
        protected int timeBetweenConnectErrorMillis = 500;
        /// <summary>
        /// 默认最大激活数
        /// </summary>
        public readonly static int DEFAULT_MAX_ACTIVE_SIZE = 8;
        public static long DEFAULT_PHY_TIMEOUT_MILLIS = -1;
        protected volatile int initialSize = 0;
        /// <summary>
        /// 上次创建链接时发生的错误
        /// </summary>
        protected volatile Exception lastCreateError;
        /// <summary>
        /// 上次创建链接时发生错误的时间
        /// </summary>
        protected long lastCreateErrorTimeMillis;
        protected long lastErrorTimeMillis;
        /// <summary>
        /// 创建链接错误重试尝试次数
        /// </summary>
        protected int connectionErrorRetryAttempts = 1;
        /// <summary>
        /// 申请连接时执行validationQuery检测连接是否有效，
        ///做了这个配置会降低性能
        /// </summary>
        protected volatile bool testOnBorrow = false;

        protected static readonly object LockObj = new object();

        public abstract void AfterPropertiesSet();

        public abstract void Dispose();

        public abstract IDbConnection GetConnection();

        public abstract IDbConnection GetConnection(string connectionString);

        /// <summary>
        /// 获得链接时，没到超时时间的重试次数
        /// </summary>
        public int NotFullTimeoutRetryCount { set; get; } = 0;

        /// <summary>
        /// 校验链接是否有效
        /// </summary>
        /// <param name="dbConnection"></param>
        public void ValidateConnection(IDbConnection dbConnection)
        {
            if (dbConnection.State == ConnectionState.Closed) throw new Exception("validateConnection: connection closed");
            //链接有效性校验器存在
            if (ValidConnectionChecker != null)
            {
                bool result = true;
                Exception error = null;
                try
                {
                    //通过校验器进行校验
                    result = ValidConnectionChecker.IsValidConnection(dbConnection, ValidationQuery, ValidationQueryTimeout);

                    lock (LockObj)
                    {
                        //如果通过校验，并且致命异常发生，则恢复致命异常的状态
                        if (result && onFatalError)
                        {
                            if (onFatalError)
                            {
                                onFatalError = false;
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return;
            }

            //链接有效性校验器不存在，自己执行sql语句校验
            if (ValidationQuery.HasText())
            {
                try
                {
                    var command = dbConnection.CreateCommand();
                    command.CommandText = ValidationQuery;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = ValidationQueryTimeout;
                    var result = command.ExecuteReader();
                    if (!result.NextResult()) throw new Exception("validationQuery didn't return a row");

                    lock (LockObj)
                    {
                        //重置致命错误状态为未发生
                        if (onFatalError)
                        {
                            if (onFatalError)
                            {
                                onFatalError = false;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    dbConnection.Close();
                }
            }
        }

        protected bool TestConnectionInternal(DruidConnectionHolder holder, IDbConnection dbConnection)
        {
            try
            {
                if (ValidConnectionChecker != null)
                {
                    bool result = true;
                    Exception error = null;

                    result = ValidConnectionChecker.IsValidConnection(dbConnection, ValidationQuery, ValidationQueryTimeout);
                    long currentTimeMillis = SbUtil.CurrentTimeMillis();
                    if (holder != null)
                    {
                        holder.LastValidTimeMillis = currentTimeMillis;
                    }
                    lock (LockObj)
                    {
                        if (result && onFatalError)
                        {
                            if (onFatalError)
                            {
                                onFatalError = false;
                            }
                        }
                    }
                    return result;
                }

                if (dbConnection.IsClose()) return false;


                if (ValidationQuery.HasText())
                {
                    try
                    {
                        var command = dbConnection.CreateCommand();
                        command.CommandText = ValidationQuery;
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = ValidationQueryTimeout;
                        var result = command.ExecuteReader();
                        if (!result.NextResult())
                        {
                            return false;
                        }

                        lock (LockObj)
                        {
                            if (onFatalError)
                            {
                                if (onFatalError)
                                {
                                    onFatalError = false;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                    finally
                    {
                        dbConnection.Close();
                    }
                }


            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        protected void SetFailContinuous(bool fail)
        {
            if (fail)
            {
                Interlocked.Exchange(ref failContinuousTimeMillis, SbUtil.CurrentTimeMillis());
            }
            else
            {
                Interlocked.Exchange(ref failContinuousTimeMillis, 0);
            }

            
            bool currentState = Interlocked.Exchange(ref failContinuous, 1) == 1;
            if (currentState == fail)
            {
                return;
            }

            if (fail)
            {
                Interlocked.Exchange(ref failContinuous, 1);
            }
            else
            {
                Interlocked.Exchange(ref failContinuous, 0);
            }
        }

        /// <summary>
        /// 设置错误错误
        /// </summary>
        /// <param name="ex"></param>
        protected void SetCreateError(Exception ex)
        {
            if (ex == null)
            {
                lock (LockObj)
                {
                    if (createError != null)
                    {
                        createError = null;
                    }
                }
                return;
            }

            Interlocked.Increment(ref createErrorCount);
            long now = SbUtil.CurrentTimeMillis();

            lock (LockObj)
            {
                createError = ex;
                lastCreateError = ex;
                lastCreateErrorTimeMillis = now;
            }
        }

        protected void Wait(int timeOut=0)
        {
            Monitor.Pulse(LockObj);
            Monitor.Wait(LockObj,timeOut);
        }

    }
}