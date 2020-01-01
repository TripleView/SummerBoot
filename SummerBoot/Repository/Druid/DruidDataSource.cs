using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using SummerBoot.Core;

namespace SummerBoot.Repository.Druid
{

    /// <summary>
    /// 移植Alibaba的druidDataSource数据库连接池
    /// </summary>
    public class DruidDataSource : DruidAbstractDataSource
    {

        public DruidDataSource(RepositoryOption option, ILogger<DruidDataSource> logger)
        {
            this.Logger = logger;
            this.RepositoryOption = option;
        }

        private readonly ILogger<DruidDataSource> Logger;


        /// <summary>
        /// 同步基元，能够使一个或多个线程等待其他线程完成各自的工作后再执行,一个典型应用场景就是启动一个服务时，主线程需要等待多个组件加载完毕，之后再继续执行。
        /// 可以参考https://www.cnblogs.com/mqxs/p/6237746.html
        /// </summary>
        private readonly CountdownEvent countdownEvent = new CountdownEvent(2);
        private long removeAbandonedCount = 0L;
        /// <summary>
        /// 连接错误计数
        /// </summary>
        private long connectErrorCount = 0L;
        private long closeTimeMillis = -1L;
        /// <summary>
        /// 连接池是否有效
        /// </summary>
        private volatile bool enable = true;
        private readonly AutoResetEvent notEmpty=new AutoResetEvent(false);
        private readonly AutoResetEvent empty=new AutoResetEvent(false);
        /// <summary>
        /// 活跃数量峰值
        /// </summary>
        private int activePeak = 0;
        private long activePeakTime = 0;
        private int notEmptyWaitThreadCount = 0;
        private int notEmptyWaitThreadPeak = 0;
        /// <summary>
        /// 连接计数
        /// </summary>
        private long connectCount = 0L;
        private long notEmptyWaitCount = 0L;
        private long notEmptyWaitNanos = 0L;
        private long closeCount = 0L;
        private long recycleCount = 0L;
        private long recycleErrorCount = 0L;
        private volatile bool closing = false;
        private bool asyncInit = false;
        public static ThreadLocal<long> waitNanosLocal = new ThreadLocal<long>();
        /// <summary>
        /// 创建数据库链接的线程
        /// </summary>
        private Thread createConnectionThread;

        /// <summary>
        /// 销毁数据库链接的线程
        /// </summary>
        private Thread destroyConnectionThread;

        private readonly RepositoryOption RepositoryOption;

        /// <summary>
        /// 数据库类型
        /// </summary>
        private Type dbType;

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        private volatile string ConnectionString;
        /// <summary>
        /// 总连接数数组
        /// </summary>
        private volatile DruidConnectionHolder[] connections;
        /// <summary>
        /// 失效的连接数数组
        /// </summary>
        private volatile DruidConnectionHolder[] evictConnections;
        /// <summary>
        /// 存活的连接数数组
        /// </summary>
        private volatile DruidConnectionHolder[] keepAliveConnections;

        /// <summary>
        /// 初始化类
        /// </summary>
        public void Init()
        {
            //如果已经创建完毕，则返回
            if (Inited) return;
            var init = false;
            try
            {
                //检查校验链接的语句
                ValidationQueryCheck();
                //创建连接池数组-失效连接数组-存活数组
                connections = new DruidConnectionHolder[maxActive];
                evictConnections = new DruidConnectionHolder[maxActive];
                keepAliveConnections = new DruidConnectionHolder[maxActive];

                Exception connectError = null;

                if (!asyncInit)
                {
                    // init connections
                    while (poolingCount < initialSize)
                    {
                        try
                        {
                            PhysicalConnectionInfo pyConnectInfo = CreatePhysicalConnection();
                            DruidConnectionHolder holder = new DruidConnectionHolder(this, pyConnectInfo);
                            connections[poolingCount++] = holder;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("init datasource error");
                            if (initExceptionThrow)
                            {
                                connectError = ex;
                                break;
                            }
                            else
                            {
                                Thread.Sleep(3000);
                            }
                        }
                    }

                    if (poolingCount > 0)
                    {
                        poolingPeak = poolingCount;
                        poolingPeakTime = SbUtil.CurrentTimeMillis();
                    }
                }

                //开启创建者线程
                CreateAndStartCreatorThread();
                //开启销毁者线程
                CreateAndStartDestroyThread();
                //阻塞线程，等创建者线程和销毁者线程创建完毕
                countdownEvent.Wait();
                //完成初始化
                init = true;
                initedTime = DateTime.Now;
                
                if (connectError != null && poolingCount == 0)
                {
                    throw connectError;
                }

                if (keepAlive)
                {
                    empty.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                Inited = true;
            }

        }

        public long CreateConnectionId()
        {
            return Interlocked.Increment(ref connectionIdSeed);
        }

        /// <summary>
        /// 获得链接
        /// </summary>
        /// <returns></returns>
        public override IDbConnection GetConnection()
        {
            return GetConnection(maxWait);
        }

        public IDbConnection GetConnection(long maxWaitMillis)
        {
            //to-do 责任链
            return GetConnectionDirect(maxWaitMillis);
        }

        public IDbConnection GetConnectionDirect(long maxWaitMillis)
        {
            //超时计数
            int notFullTimeoutRetryCnt = 0;
            for (; ; )
            {
                // handle notFullTimeoutRetry
                DruidDbConnection druidDbConnection;
                try
                {
                    druidDbConnection = GetConnectionInternal(maxWaitMillis);
                }
                catch (Exception ex)
                {
                    //如果超时计数没达到设定的上限，并且连接池未满，则继续重试
                    if (notFullTimeoutRetryCnt <= this.NotFullTimeoutRetryCount && !IsFull())
                    {
                        notFullTimeoutRetryCnt++;
                        Logger.LogWarning("get connection timeout retry : " + notFullTimeoutRetryCnt);
                        continue;
                    }

                    throw ex;
                }

                //从池中取出链接前进行检验
                if (testOnBorrow)
                {
                    bool validate = TestConnectionInternal(druidDbConnection.Holder, druidDbConnection);
                    if (!validate)
                    {

                        Logger.LogDebug("skip not validate connection.");
                        //如果校验失败，则彻底抛弃这个链接
                        IDbConnection realConnection = druidDbConnection.DbConnection;
                        DiscardConnection(realConnection);
                        continue;
                    }
                }
                //从池中取出链接前不校验
                else
                {
                    IDbConnection realConnection = druidDbConnection.DbConnection;
                    if (druidDbConnection.DbConnection.IsClose())
                    {
                        DiscardConnection(null); // 传入null，避免重复关闭
                        continue;
                    }
                    //如果开启空闲时校验
                    if (testWhileIdle)
                    {
                        DruidConnectionHolder holder = druidDbConnection.Holder;
                        var currentTimeMillis = SbUtil.CurrentTimeMillis();
                        //上次激活时间
                        var lastActiveTimeMillis = holder.LastActiveTimeMillis;
                        //上次存活时间
                        var lastKeepTimeMillis = holder.LastKeepTimeMillis;
                        //存活时间在激活时间之后，则更新激活时间
                        if (lastKeepTimeMillis > lastActiveTimeMillis)
                        {
                            lastActiveTimeMillis = lastKeepTimeMillis;
                        }
                        //空闲时间=现在时间-上次激活时间
                        var idleMillis = currentTimeMillis - lastActiveTimeMillis;

                        long timeBetweenEvictionRunsMillis = this.timeBetweenEvictionRunsMillis;

                        if (timeBetweenEvictionRunsMillis <= 0)
                        {
                            timeBetweenEvictionRunsMillis = DEFAULT_TIME_BETWEEN_EVICTION_RUNS_MILLIS;
                        }
                        //如果空闲时间>空闲连接回收器线程运行期间休眠的时间值|空闲时间<0,则进行校验
                        if (idleMillis >= timeBetweenEvictionRunsMillis
                                || idleMillis < 0 // unexcepted branch
                                )
                        {
                            bool validate = TestConnectionInternal(druidDbConnection.Holder, druidDbConnection.DbConnection);
                            if (!validate)
                            {
                                //校验不通过，则抛弃
                                Logger.LogDebug("skip not validate connection.");
                                DiscardConnection(realConnection);
                                continue;
                            }
                        }
                    }
                }
                //删除被抛弃的链接
                if (removeAbandoned)
                {
                    druidDbConnection.SetConnectedTimeNano();
                    lock (LockObj)
                    {
                        activeConnections.Add(druidDbConnection, PRESENT);
                    }
                }

                return druidDbConnection;
            }
        }

        /// <summary>
        /// 不进行回收,抛弃连接
        /// </summary>
        /// <param name="realConnection"></param>
        public void DiscardConnection(IDbConnection realConnection)
        {
            realConnection.LogAndClose();

            lock (LockObj)
            {
                activeCount--;
                discardCount++;
                if (activeCount <= minIdle)
                {
                    empty.Set();
                }
            }
        }

        private DruidDbConnection GetConnectionInternal(long maxWait)
        {
            //如果连接池已关闭
            if (closed)
            {
                //增加连接错误计数
                Interlocked.Increment(ref connectErrorCount);
                throw new Exception("dataSource already closed at" + new DateTime(closeTimeMillis));
            }
            //如果连接池已失效
            if (!enable)
            {
                //增加连接错误计数
                Interlocked.Increment(ref connectErrorCount);

                throw new Exception("dataSource is disable");
            }
            int maxWaitThreadCount = this.maxWaitThreadCount;
            long nanos = maxWait * 1000000;

            DruidConnectionHolder holder;
            //以下待实现
            for (var createDirect = false; ;)
            {
                if (createDirect)
                {
                    //更新createStartNanos的值
                    Interlocked.Exchange(ref createStartNanos, SbUtil.NanoTime());


                    //如果creatingCount创建数量=0，则赋值为1
                    if (Interlocked.CompareExchange(ref creatingCount, 1, 0) > 0)
                    {
                        //创建具体链接
                        PhysicalConnectionInfo pyConnInfo = CreatePhysicalConnection();
                        //创建链接容器
                        holder = new DruidConnectionHolder(this, pyConnInfo);
                        //更新上次激活时间
                        holder.LastActiveTimeMillis = SbUtil.CurrentTimeMillis();
                        //增加创建计数
                        Interlocked.Increment(ref createCount);
                        //增加直接创建链接计数
                        Interlocked.Increment(ref directCreateCount);

                        Logger.LogDebug("conn-direct_create ");

                        //是否抛弃
                        bool discard = false;
                        lock (LockObj)
                        {
                            try
                            {
                                //如果活跃数量<最大活跃数量
                                if (activeCount < maxActive)
                                {
                                    //活跃数量自增
                                    activeCount++;
                                    //如果活跃数量大于活跃数量峰值，则更新峰值，更新峰值时间
                                    if (activeCount > activePeak)
                                    {
                                        activePeak = activeCount;
                                        activePeakTime = SbUtil.CurrentTimeMillis();
                                    }

                                    break;
                                }
                                //活跃数量>最大活跃数量,则抛弃
                                else
                                {

                                    discard = true;
                                }
                            }
                            finally
                            {

                            }
                        }
                        //如果抛弃，直接关闭链接
                        if (discard)
                        {
                            pyConnInfo.DbConnection.LogAndClose();
                        }
                    }
                }

                lock (LockObj)
                {
                    try
                    {
                        if (maxWaitThreadCount > 0
                            && notEmptyWaitThreadCount >= maxWaitThreadCount)
                        {
                            //增加连接错误计数
                            Interlocked.Increment(ref connectErrorCount);
                            throw new Exception("maxWaitThreadCount " + maxWaitThreadCount +
                                                ", current wait Thread count ");
                        }

                        //发生致命错误
                        if (onFatalError
                            && onFatalErrorMaxActive > 0
                            && activeCount >= onFatalErrorMaxActive)
                        {
                            //增加连接错误计数
                            Interlocked.Increment(ref connectErrorCount);


                            StringBuilder errorMsg = new StringBuilder();
                            errorMsg.Append("onFatalError, activeCount ")
                                .Append(activeCount)
                                .Append(", onFatalErrorMaxActive ")
                                .Append(onFatalErrorMaxActive);

                            if (lastFatalErrorTimeMillis > 0)
                            {
                                errorMsg.Append(", time '")
                                    .Append("")
                                    .Append("'");
                            }

                            throw new Exception(
                                errorMsg.ToString(), lastFatalError);
                        }

                        //连接计数增加
                        connectCount++;

                        if (maxWait > 0)
                        {
                            holder = PollLast(nanos);
                        }
                        else
                        {
                            holder = TakeLast();
                        }

                        if (holder != null)
                        {
                            //活跃计数增加
                            activeCount++;
                            //活跃计数>活跃峰值，则更新峰值和峰值时间
                            if (activeCount > activePeak)
                            {
                                activePeak = activeCount;
                                activePeakTime = SbUtil.CurrentTimeMillis();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Interlocked.Increment(ref connectErrorCount);
                        throw new Exception(e.Message, e);
                    }
                }

                break;
            }

            //如果容器为空
            if (holder == null)
            {
                long waitNanos = waitNanosLocal.Value;

                if (this.createError != null)
                {
                    throw createError;
                }
            }


            holder.IncrementUseCount();

            var connection = new DruidDbConnection(holder);
            return connection;
        }

        public bool IsFailContinuous()
        {
            lock (LockObj)
            {
                return failContinuous == 1;
            }
        }
        private DruidConnectionHolder PollLast(long nanos)
        {
            long estimate = nanos;

            for (; ; )
            {
                if (poolingCount == 0)
                {
                    empty.Set(); // send signal to CreateThread create connection

                    if (failFast && IsFailContinuous())
                    {
                        throw createError;
                    }

                    if (estimate <= 0)
                    {
                        waitNanosLocal.Value = nanos - estimate;
                        return null;
                    }

                    notEmptyWaitThreadCount++;
                    if (notEmptyWaitThreadCount > notEmptyWaitThreadPeak)
                    {
                        notEmptyWaitThreadPeak = notEmptyWaitThreadCount;
                    }

                    try
                    {
                        long startEstimate = estimate;
                        int estimateInt = Convert.ToInt32(estimate);
                        notEmpty.WaitOne(estimateInt); // signal by
                                                       // recycle or
                                                       // creator
                        notEmptyWaitCount++;
                        notEmptyWaitNanos += (startEstimate - estimate);

                        if (!enable)
                        {
                            Interlocked.Increment(ref connectErrorCount);

                            if (disableException != null)
                            {
                                throw disableException;
                            }

                        }
                    }
                    catch (Exception ie)
                    {
                        notEmpty.Set(); // propagate to non-interrupted thread
                        notEmptySignalCount++;
                        throw ie;
                    }
                    finally
                    {
                        notEmptyWaitThreadCount--;
                    }

                    if (poolingCount == 0)
                    {
                        if (estimate > 0)
                        {
                            continue;
                        }

                        waitNanosLocal.Value = (nanos - estimate);
                        return null;
                    }
                }

                poolingCount--;
                DruidConnectionHolder last = connections[poolingCount];
                connections[poolingCount] = null;

                long waitNanos = nanos - estimate;
                last.LastNotEmptyWaitNanos = waitNanos;

                return last;
            }
        }


        /// <summary>
        /// 回收连接
        /// </summary>
        /// <param name="pooledConnection"></param>
        public void Recycle(DruidDbConnection pooledConnection)
        {
            DruidConnectionHolder holder = pooledConnection.Holder;

            if (holder == null)
            {
                Logger.LogWarning("connectionHolder is null");
                return;
            }

            var physicalConnection = holder.Connection;



            var testOnReturn = this.testOnReturn;

            try
            {
                // check need to rollback?


                // reset holder, restore default settings, clear warnings
                //判断回收的线程和取走的线程是否是同一个
                var isSameThread = pooledConnection.OwnerThread == Thread.CurrentThread;
                if (!isSameThread)
                {
                    lock (LockObj)
                    {
                        holder.Reset();
                    }
                }
                else
                {
                    holder.Reset();
                }

                //如果holder已被抛弃，直接返回
                if (holder.Discard)
                {
                    return;
                }

                //物理连接的最大使用次数>0&&holder的使用次数>物理连接的最大使用次数
                if (phyMaxUseCount > 0 && holder.UseCount >= phyMaxUseCount)
                {
                    //抛弃链接
                    DiscardConnection(holder.Connection);
                    return;
                }

                //如果链接已关闭
                if (physicalConnection.IsClose())
                {

                    lock (LockObj)
                    {
                        //活跃次数自减
                        activeCount--;
                        //关闭次数自增
                        closeCount++;
                    }

                    return;
                }

                //回收时进行校验
                if (testOnReturn)
                {
                    bool validate = TestConnectionInternal(holder, physicalConnection);
                    if (!validate)
                    {
                        //关闭链接
                        physicalConnection.LogAndClose();
                        //销毁计数自增
                        Interlocked.Increment(ref destroyCount);

                        lock (LockObj)
                        {
                            //活跃次数自减
                            activeCount--;
                            //关闭次数自增
                            closeCount++;
                        }

                        return;
                    }
                }

                //如果数据源已失效
                if (!enable)
                {
                    //抛弃链接
                    DiscardConnection(holder.Connection);
                    return;
                }

                bool result;
                long currentTimeMillis = SbUtil.CurrentTimeMillis();

                if (phyTimeoutMillis > 0)
                {
                    long phyConnectTimeMillis = currentTimeMillis - holder.ConnectTimeMillis;
                    if (phyConnectTimeMillis > phyTimeoutMillis)
                    {
                        DiscardConnection(holder.Connection);
                        return;
                    }
                }

                lock (LockObj)
                {
                    try
                    {
                        activeCount--;
                        closeCount++;
                        result = PutLast(holder, currentTimeMillis);
                        recycleCount++;
                        if (!result)
                        {
                            holder.Connection.LogAndClose();
                            Logger.LogInformation("connection recyle failed.");

                        }
                    }
                    catch (Exception e)
                    {

                        if (!holder.Discard)
                        {
                            this.DiscardConnection(physicalConnection);
                            holder.Discard = true;
                        }

                        Logger.LogError("recyle error:" + e.Message);
                        Interlocked.Increment(ref recycleErrorCount);

                    }
                }


            }
            catch (Exception e)
            {

            }
        }

        private bool PutLast(DruidConnectionHolder e, long lastActiveTimeMillis)
        {
            if (poolingCount >= maxActive)
            {
                return false;
            }

            e.LastActiveTimeMillis = lastActiveTimeMillis;
            connections[poolingCount] = e;
            poolingCount++;


            if (poolingCount > poolingPeak)
            {
                poolingPeak = poolingCount;
                poolingPeakTime = lastActiveTimeMillis;
            }

            notEmpty.Set();
            notEmptySignalCount++;

            return true;
        }


        private DruidConnectionHolder TakeLast()
        {
            try
            {
                while (poolingCount == 0)
                {
                    empty.Set(); // send signal to CreateThread create connection

                    if (failFast && IsFailContinuous())
                    {
                        throw (createError);
                    }

                    notEmptyWaitThreadCount++;
                    if (notEmptyWaitThreadCount > notEmptyWaitThreadPeak)
                    {
                        notEmptyWaitThreadPeak = notEmptyWaitThreadCount;
                    }
                    try
                    {
                        notEmpty.WaitOne(); // signal by recycle or creator
                    }
                    finally
                    {
                        notEmptyWaitThreadCount--;
                    }
                    notEmptyWaitCount++;

                    if (!enable)
                    {
                        Interlocked.Increment(ref connectErrorCount);
                        if (disableException != null)
                        {
                            throw disableException;
                        }

                        throw new Exception();
                    }
                }
            }
            catch (Exception ie)
            {
                notEmpty.Set(); // propagate to non-interrupted thread
                notEmptySignalCount++;
                throw ie;
            }

            poolingCount--;

            DruidConnectionHolder last = connections[poolingCount];
            connections[poolingCount] = null;

            return last;
        }

        /// <summary>
        /// 判断连接数量是否已满，即池中链接数+活跃连接数>最大链接数
        /// </summary>
        /// <returns></returns>
        public bool IsFull()
        {
            lock (LockObj)
            {
                return this.poolingCount + this.activeCount >= this.maxActive;
            }
        }

        public override IDbConnection GetConnection(string connectionString)
        {
            if (connectionString.IsNullOrWhiteSpace()) throw new Exception("connectionString is empty");

            return GetConnection();
        }

        /// <summary>
        /// 生成实例后执行的方法
        /// </summary>
        public override void AfterPropertiesSet()
        {
            this.Init();
        }

        public override void Dispose()
        {

        }

        /// <summary>
        /// 创建和启动创建者线程
        /// </summary>
        protected void CreateAndStartCreatorThread()
        {
            if (CreateTimer == null)
            {
                string threadName = "Druid-ConnectionPool-Create-" + RuntimeHelpers.GetHashCode(this);
                createConnectionThread = new Thread(CreateConnection)
                {
                    IsBackground = true,
                    Name = threadName
                };
                createConnectionThread.Start();
                return;
            }
            
            countdownEvent.Signal();
        }

        /// <summary>
        /// 创建和启动销毁者线程
        /// </summary>
        protected void CreateAndStartDestroyThread()
        {

            string threadName = "Druid-ConnectionPool-Destroy-" + RuntimeHelpers.GetHashCode(this);
            createConnectionThread = new Thread(DestroyConnection)
            {
                IsBackground = true,
                Name = threadName
            };
            createConnectionThread.Start();
            return;
            countdownEvent.Signal();
        }

        /// <summary>
        /// 创建链接
        /// </summary>
        public void CreateConnection()
        {
            countdownEvent.Signal();
            long lastDiscardCount = 0;
            int errorCount = 0;
            for (; ; )
            {
                //先判断哪些情况不可以创建链接
                lock (LockObj)
                {
                    long discardCount =this.discardCount;
                    //失效变量
                    bool discardChanged = discardCount - lastDiscardCount > 0;
                    lastDiscardCount = discardCount;

                    try
                    {
                        //创建者线程是否等待,默认为等待
                        bool emptyWait = true;

                        //如果创建没错误，并且连接池数量为0，没有新增失效数量，则不等待
                        if (createError != null
                            && poolingCount == 0
                            && !discardChanged)
                        {
                            emptyWait = false;
                        }

                        if (emptyWait
                            && asyncInit && createCount < initialSize)
                        {
                            emptyWait = false;
                        }

                        if (emptyWait)
                        {
                            // 必须存在线程等待，才创建连接
                            //如果连接池数量>等待数量&！(活跃数量+连接池数量<最小空闲数量)
                            if (poolingCount >= notEmptyWaitThreadCount //
                                && (!(keepAlive && activeCount + poolingCount < minIdle))
                                && !IsFailContinuous()
                            )
                            {
                                empty.WaitOne();
                            }

                            // 防止创建超过maxActive数量的连接
                            if (activeCount + poolingCount >= maxActive)
                            {
                                empty.WaitOne();
                                continue;
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        lastCreateError = e;
                        lastErrorTimeMillis = SbUtil.CurrentTimeMillis();

                        if (!closing)
                        {
                            Logger.LogError("create connection Thread Interrupted, url: ");
                        }
                        break;
                    }
                }
                
                //确认可以创建链接了

                PhysicalConnectionInfo connection = null;

                try
                {
                    //创建链接
                    connection = CreatePhysicalConnection();
                }
                catch (Exception e)
                {
                    Logger.LogError(e.Message);
                    //错误计数增加
                    errorCount++;
                    //错误计数>链接错误重试尝试次数
                    if (errorCount > connectionErrorRetryAttempts && timeBetweenConnectErrorMillis > 0)
                    {
                        //故障切换重试尝试
                        // fail over retry attempts
                        SetFailContinuous(true);
                        if (failFast)
                        {
                            lock (LockObj)
                            {
                                notEmpty.Set();
                            }
                        }

                        //失败后中断
                        if (breakAfterAcquireFailure)
                        {
                            break;
                        }

                        //如果失败后不中断，则休眠一段时间后继续
                        try
                        {
                            Thread.Sleep(timeBetweenConnectErrorMillis);
                        }
                        catch (ThreadInterruptedException interruptEx)
                        {
                            break;
                        }
                    }
                }

                //如果链接为null，也继续
                if (connection == null)
                {
                    continue;
                }

                //把结果放进线程池里
                bool result = Put(connection);
                if (!result)
                {
                    //如果放进线程池失败了，则关闭链接
                    connection.DbConnection.LogAndClose();
                    Logger.LogInformation("put physical connection to pool failed.");
                }
            }
        }

        /// <summary>
        /// 销毁连接
        /// </summary>
        public void DestroyConnection()
        {
            countdownEvent.Signal();

            for (; ; )
            {
                // 从前面开始删除
                try
                {
                    if (closed)
                    {
                        break;
                    }

                    //回收线程休眠
                    if (timeBetweenEvictionRunsMillis > 0)
                    {
                        Thread.Sleep(timeBetweenEvictionRunsMillis);
                    }
                    else
                    {
                        Thread.Sleep(1000); //
                    }

                    //这里可能有个坑
                    //Thread.CurrentThread.Interrupt();

                    //启动销毁任务
                    Task.Run(()=>DestroyTask());
                }
                catch (ThreadInterruptedException e)
                {
                    break;
                }
            }
        }

        //暂未实现
        //public void CreateConnectionTask()
        //{
        //    for (; ; )
        //    {
        //        int errorCount = 0;
        //        lock (LockObj)
        //        {
                     
        //            if (closed || closing)
        //            {
        //                clearCreateTask(taskId);
        //                return;
        //            }

        //            bool emptyWait = true;

        //            if (createError != null && poolingCount == 0)
        //            {
        //                emptyWait = false;
        //            }
        //            if (emptyWait)
        //            {
        //                // 必须存在线程等待，才创建连接
        //                if (poolingCount >= notEmptyWaitThreadCount //
        //                    && (!(keepAlive && activeCount + poolingCount < minIdle)) // 在keepAlive场景不能放弃创建
        //                    && (!initTask) // 线程池初始化时的任务不能放弃创建
        //                    && !IsFailContinuous() // failContinuous时不能放弃创建，否则会无法创建线程
        //                    && !isOnFatalError() // onFatalError时不能放弃创建，否则会无法创建线程
        //                )
        //                {
        //                    clearCreateTask(taskId);
        //                    return;
        //                }

        //                // 防止创建超过maxActive数量的连接
        //                if (activeCount + poolingCount >= maxActive)
        //                {
        //                    clearCreateTask(taskId);
        //                    return;
        //                }
        //            }
        //        }




        //        PhysicalConnectionInfo physicalConnection = null;

        //        try
        //        {
        //            physicalConnection = CreatePhysicalConnection();
        //        }
        //        catch (Exception e)
        //        {
        //            Logger.LogError("create connection OutOfMemoryError, out memory. ");
               

        //            errorCount++;
        //            if (errorCount > connectionErrorRetryAttempts && timeBetweenConnectErrorMillis > 0)
        //            {
        //                // fail over retry attempts
        //                SetFailContinuous(true);
        //                if (failFast)
        //                {
        //                    lock (LockObj)
        //                    {
        //                        notEmpty.Set();
        //                    }
        //                }

        //                if (breakAfterAcquireFailure)
        //                {

        //                    lock (LockObj)
        //                    {
        //                        clearCreateTask(taskId);
        //                    }
                   
        //                    return;
        //                }

        //               errorCount = 0; // reset errorCount
        //                if (closing || closed)
        //                {
        //                    lock (LockObj)
        //                    {
        //                        clearCreateTask(taskId);
        //                    }
                           
                   
        //                    return;
        //                }

        //                createSchedulerFuture = createScheduler.schedule(this, timeBetweenConnectErrorMillis, TimeUnit.MILLISECONDS);
        //                return;
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Logger.LogError("create connection SQLException, url: ");
              

        //            errorCount++;
        //            if (errorCount > connectionErrorRetryAttempts && timeBetweenConnectErrorMillis > 0)
        //            {
        //                // fail over retry attempts
        //                SetFailContinuous(true);
        //                if (failFast)
        //                {

        //                    lock (LockObj)
        //                    {
        //                        notEmpty.Set();
        //                    }

        //                }

        //                if (breakAfterAcquireFailure)
        //                {

        //                    lock (LockObj)
        //                    {
        //                        clearCreateTask(taskId);
        //                    }
        //                    return;
        //                }

        //                errorCount = 0; // reset errorCount
        //                if (closing || closed)
        //                {

        //                    lock (LockObj)
        //                    {
        //                        clearCreateTask(taskId);
        //                    }

        //                    return;
        //                }

        //                createSchedulerFuture = createScheduler.schedule(this, timeBetweenConnectErrorMillis, TimeUnit.MILLISECONDS);
        //                return;
        //            }
        //        }

        //        if (physicalConnection == null)
        //        {
        //            continue;
        //        }

        //        physicalConnection.createTaskId = taskId;
        //        bool result = Put(physicalConnection);
        //        if (!result)
        //        {
        //            physicalConnection.DbConnection.LogAndClose();
        //            Logger.LogInformation("put physical connection to pool failed.");
        //        }
        //        break;
        //    }
        //}

        /// <summary>
        /// 暂未实现
        /// </summary>
        private void clearCreateTask()
        {

        }

        public void DestroyTask()
        {
            //缩小线程池容量
            Shrink(true, keepAlive);
            //如果是否移除失效的链接为真
            if (removeAbandoned)
            {
                //移除失效的链接
                RemoveAbandoned();
            }
        }

        public string Dump()
        {
            lock (LockObj)
            {
                return this.ToString();
            }
        }

        /// <summary>
        /// 移除被抛弃的链接
        /// </summary>
        /// <returns></returns>
        public int RemoveAbandoned()
        {
            //移除计数
            var removeCount = 0;

            var currrentNanos = SbUtil.NanoTime();

            var abandonedList = new List<DruidDbConnection>();

            lock (LockObj)
            {
                var keys = activeConnections.Keys.ToList();

                for (var i = keys.Count - 1; i >= 0; i--)
                {
                    var connection = keys[i];
                    if (connection.Running) continue;
                    //计算从建立连接开始到现在的时间差
                    long timeMillis = (currrentNanos - connection.ConnectedTimeNano) / (1000 * 1000);
                    //如果这个时间差>设定的时间差，则抛弃
                    if (timeMillis >= removeAbandonedTimeoutMillis)
                    {
                        //这里可能有坑
                        keys.RemoveAt(i);
                        abandonedList.Add(connection);
                    }
                }

            }

            if (abandonedList.Count > 0)
            {
                for (var i = 0; i < abandonedList.Count; i++)
                {
                    lock (LockObj)
                    {
                        var connection = abandonedList[i];
                        if (connection.Disable) continue;
                        connection.LogAndClose();
                        connection.SetAbandond();
                        removeAbandonedCount++;
                        removeCount++;
                        if (logAbandoned)
                        {
                        }
                    }
                }

            }

            return removeCount;
        }

        /// <summary>
        /// 收缩线程池
        /// </summary>
        /// <param name="checkTime">是否校验时间</param>
        /// <param name="keepAlive"></param>
        public void Shrink(bool checkTime, bool keepAlive)
        {
            bool needFill = false;
            //移除的数量
            int evictCount = 0;
            //存活的数量
            int keepAliveCount = 0;
            //致命错误的增量
            int fatalErrorIncrement = fatalErrorCount - fatalErrorCountLastShrink;
            fatalErrorCountLastShrink = fatalErrorCount;
            lock (LockObj)
            {
                 needFill = false;
                //移除的数量
                 evictCount = 0;
                //存活的数量
                 keepAliveCount = 0;
                //致命错误的增量
                fatalErrorIncrement = fatalErrorCount - fatalErrorCountLastShrink;
                fatalErrorCountLastShrink = fatalErrorCount;

                if (!Inited)
                {
                    return;
                }
                //需要查验的数量=池中的数量-允许的最小空闲数量，需要查验意味着可能被移除
                var checkCount = poolingCount - minIdle;
                long currentTimeMillis = SbUtil.CurrentTimeMillis();

                for (int i = 0; i < poolingCount; ++i)
                {
                    DruidConnectionHolder connection = connections[i];

                    //(发生致命错误||致命错误增量>0)&&(上次发生致命错误的时间>链接的创建时间)
                    if ((onFatalError || fatalErrorIncrement > 0) && (lastFatalErrorTimeMillis > connection.ConnectTimeMillis))
                    {
                        keepAliveConnections[keepAliveCount++] = connection;
                        continue;
                    }

                    //如果校验时间
                    if (checkTime)
                    {
                        //如果设定了连接数据库后多长时间后一定要强制移除
                        if (phyTimeoutMillis > 0)
                        {
                            //计算时间差
                            long phyConnectTimeMillis = currentTimeMillis - connection.ConnectTimeMillis;
                            //时间差大于设定值，移除该链接
                            if (phyConnectTimeMillis > phyTimeoutMillis)
                            {
                                evictConnections[evictCount++] = connection;
                                continue;
                            }
                        }

                        //IdbConnection的空闲时间=当前时间-上次激活时间
                        long idleMillis = currentTimeMillis - connection.LastActiveTimeMillis;

                        //如果空闲时间<允许的空闲时间的下限值&&空闲时间<存活时间，则跳过
                        if (idleMillis < minEvictableIdleTimeMillis
                                && idleMillis < keepAliveBetweenTimeMillis
                        )
                        {
                            break;
                        }

                        //如果空闲时间>=允许的空闲时间的下限值
                        if (idleMillis >= minEvictableIdleTimeMillis)
                        {
                            //优先移除数组中前面的链接， 举个栗子，池中有10个链接，允许空闲的最小连接数是3，那么另外7个都可能被移除，优先移除前面7个，剩后面3个
                            if (i < checkCount)
                            {
                                evictConnections[evictCount++] = connection;
                                continue;
                            }
                            //如果空闲时间>=允许的空闲时间的上限值，移除
                            if (idleMillis > maxEvictableIdleTimeMillis)
                            {
                                evictConnections[evictCount++] = connection;
                                continue;
                            }
                        }
                        //如果开启了存活配置&&空闲时间>允许的数据库链接的存活时间，则把链接放到存活数组里
                        if (keepAlive && idleMillis >= keepAliveBetweenTimeMillis)
                        {
                            keepAliveConnections[keepAliveCount++] = connection;
                        }
                    }
                    else
                    {
                        //优先移除数组中前面的链接， 举个栗子，池中有10个链接，允许空闲的最小连接数是3，那么另外7个都可能被移除，优先移除前面7个，剩后面3个
                        if (i < checkCount)
                        {
                            evictConnections[evictCount++] = connection;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                //真正移除数量=移除数量+存活数量
                int removeCount = evictCount + keepAliveCount;
                //真正移除数量>0，则进行移除操作
                if (removeCount > 0)
                {
                    //保留有效的链接=》通过复制数组来实现
                    Array.Copy(connections, removeCount, connections, 0, poolingCount - removeCount);
                    //移除操作=>通过给数组里那些没用的链接全部赋值为null来实现
                    Array.Fill(connections, null, poolingCount - removeCount, poolingCount);
                    //池中数量减去真正移除数量
                    poolingCount -= removeCount;
                }
                keepAliveCheckCount += keepAliveCount;

                //存活&&(池中的数量+激活的数量<池中的允许的最小空闲数量)为true，则需要填充
                if (keepAlive && poolingCount + activeCount < minIdle)
                {
                    needFill = true;
                }
            }

            //移除数量>0,
            if (evictCount > 0)
            {
                for (int i = 0; i < evictCount; ++i)
                {
                    DruidConnectionHolder item = evictConnections[i];
                    var connection = item.Connection;
                    //关掉所有链接
                    connection.LogAndClose();
                    //增加销毁计数
                    Interlocked.Increment(ref destroyCount);
                }
                //清空存放移除链接的数组=》全部赋值为null
                Array.Fill(evictConnections, null);
            }

            //存活数量>0
            if (keepAliveCount > 0)
            {
                // keep order
                for (int i = keepAliveCount - 1; i >= 0; --i)
                {
                    DruidConnectionHolder holer = keepAliveConnections[i];
                    var connection = holer.Connection;
                    //增加存活计数
                    holer.IncrementKeepAliveCheckCount();

                    //判断链接是否有效
                    bool validate = false;
                    try
                    {
                        this.ValidateConnection(connection);
                        validate = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogDebug(ex.Message);
                    }
                    //无效标志
                    bool discard = !validate;
                    //如果有效状态
                    if (validate)
                    {
                        //更新最后一次有效的时间
                        holer.LastKeepTimeMillis = SbUtil.CurrentTimeMillis();
                        //放入线程池
                        bool putOk = Put(holer, 0L);
                        //如果放入线程池失败,则变为报废状态
                        if (!putOk)
                        {
                            discard = true;
                        }
                    }
                    //如果是报废状态
                    if (discard)
                    {
                        try
                        {
                            //关闭链接
                            connection.LogAndClose();
                        }
                        catch (Exception e)
                        {
                            // skip
                        }

                        lock (LockObj)
                        {
                            //报废计数累加
                            discardCount++;

                            //如果激活数量+池中已有数量<=池中允许的最小空闲数量，则启动线程增加链接数
                            if (activeCount + poolingCount <= minIdle)
                            {
                                empty.Set();
                            }
                        }

                    }
                }
                //清空存放存活链接的数组=》全部赋值为null
                Array.Fill(keepAliveConnections, null);
            }

            //需要填充
            if (needFill)
            {
                lock (LockObj)
                {
                    //填充数量=池中允许的最小空闲数量-已激活的数量-池中现有的数量
                    int fillCount = minIdle - (activeCount + poolingCount + createTaskCount);
                    for (int i = 0; i < fillCount; ++i)
                    {
                        //启动线程增加链接
                        empty.Set();
                    }
                }
            }
            //发生致命错误||错误增量>0
            else if (onFatalError || fatalErrorIncrement > 0)
            {
                lock (LockObj)
                {
                    //启动线程增加链接
                    empty.Set();
                }
            }
        }
        /// <summary>
        /// 把链接放入线程池
        /// </summary>
        /// <param name="physicalConnectionInfo"></param>
        /// <returns></returns>
        protected bool Put(PhysicalConnectionInfo physicalConnectionInfo)
        {
            DruidConnectionHolder holder = null;
            try
            {
                holder = new DruidConnectionHolder(this, physicalConnectionInfo);
            }
            catch (Exception ex)
            {
                Logger.LogWarning("create connection holder error");
                return false;
            }

            return Put(holder, 123);
        }

        /// <summary>
        /// 把链接放入线程池
        /// </summary>
        /// <param name="druidConnectionHolder"></param>
        /// <param name="createTaskId"></param>
        /// <returns></returns>
        private bool Put(DruidConnectionHolder druidConnectionHolder, long createTaskId)
        {
            lock (LockObj)
            {
                //池中数量>最大活跃数量，直接返回
                if (poolingCount >= maxActive)
                {
                    return false;
                }
                //赋值
                connections[poolingCount] = druidConnectionHolder;
                //池中数量自增
                poolingCount++;

                //如果池中数量大于池中数量的峰值，则更新峰值和记录达到峰值的时间
                if (poolingCount > poolingPeak)
                {
                    poolingPeak = poolingCount;
                    poolingPeakTime = SbUtil.CurrentTimeMillis();
                }
                //唤醒消费者线程
                notEmpty.Set();
                //唤醒次数自增
                notEmptySignalCount++;
            }

            return true;
        }

        /// <summary>
        /// 创建物理链接
        /// </summary>
        /// <returns></returns>
        public PhysicalConnectionInfo CreatePhysicalConnection()
        {
            IDbConnection conn = null;

            long connectStartNanos = SbUtil.NanoTime();
            long connectedNanos, initedNanos, validatedNanos;
            //开始创建链接的时间
            Interlocked.Exchange(ref createStartNanos, connectStartNanos);
            //创建链接计数加一
            Interlocked.Increment(ref createCount);
            try
            {
                conn = CreatePhysicalConnection2();
                //连接数据库的时间
                connectedNanos = SbUtil.NanoTime();
                if (conn == null)
                {
                    throw new Exception("connect error, connectionString" + RepositoryOption.ConnectionString);
                }
                //初始化物理链接
                InitPhysicalConnection(conn);
                //记录初始化时间
                initedNanos = SbUtil.NanoTime();
                //校验链接
                ValidateConnection(conn);
                //记录校验时间
                validatedNanos = SbUtil.NanoTime();

            }
            catch (Exception e)
            {
                //记录错误信息
                SetCreateError(e);
                //关闭链接
                conn.LogAndClose();
                //创建错误计数自增
                Interlocked.Increment(ref createErrorCount);
                throw;
            }
            finally
            {
                //当前时间-链接开始时间得到时间差
                long nano = SbUtil.NanoTime() - connectStartNanos;
                //增加创建时间间隔
                CreateTimespan += nano;
                //创建计数自增
                Interlocked.Decrement(ref createCount);
            }

            //返回包装后的链接信息
            return new PhysicalConnectionInfo(conn, connectStartNanos, connectedNanos, initedNanos, validatedNanos);
        }

        /// <summary>
        /// 初始化物理链接
        /// </summary>
        public void InitPhysicalConnection(IDbConnection dboConnection)
        {

        }

        public IDbConnection CreatePhysicalConnection2()
        {
            //利用反射创建真实链接
            var dbConnection = (IDbConnection)RepositoryOption.DbConnectionType.CreateInstance(null);

            dbConnection.ConnectionString = RepositoryOption.ConnectionString;
            dbConnection.Open();
            //创建计数增加
            Interlocked.Increment(ref createCount);
            return dbConnection;
        }

        //public static void CreateThreadRun()
        //{
        //    countdownEvent.Signal();
        //}

        /// <summary>
        /// 检查校验链接的语句
        /// </summary>
        private void ValidationQueryCheck()
        {
            if (!(testOnBorrow || testOnReturn || testWhileIdle))
            {
                return;
            }

            if (this.ValidConnectionChecker != null)
            {
                return;
            }

            if (this.ValidationQuery.HasText())
            {
                return;
            }

            var errorMessage = "";

            if (testOnBorrow)
            {
                errorMessage += "testOnBorrow is true, ";
            }

            if (testOnReturn)
            {
                errorMessage += "testOnReturn is true, ";
            }

            if (testWhileIdle)
            {
                errorMessage += "testWhileIdle is true, ";
            }
            Logger.LogError(errorMessage + "validationQuery not set");
        }

    }
}