using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using SummerBoot.Core;

namespace SummerBoot.Cache
{
    public class RedisCacheWriter : IRedisCacheWriter
    {
        /// <summary>
        /// 实际操作redis的组件
        /// </summary>
        private IConnectionMultiplexer ConnectionMultiplexer { set; get; }

        /// <summary>
        /// 两次锁定请求尝试之间的睡眠时间,不可为null，如果要禁用，请使用timeSpan.zero
        /// </summary>
        private TimeSpan SleepTime { set; get; }

        private ILogger<RedisCacheWriter> logger { set; get; } =
            new LoggerFactory().CreateLogger<RedisCacheWriter>();

        public RedisCacheWriter(IConnectionMultiplexer connectionMultiplexer, TimeSpan sleepTime)
        {
            SbAssert.NotNull(connectionMultiplexer, "ConnectionMultiplexer不能为null");
            SbAssert.NotNull(sleepTime, "sleepTime不能为null");

            this.ConnectionMultiplexer = connectionMultiplexer;
            this.SleepTime = sleepTime;
        }

        public RedisCacheWriter(IConnectionMultiplexer connectionMultiplexer) : this(connectionMultiplexer, TimeSpan.Zero)
        {
        }

        public byte[] Get(string name, byte[] key)
        {
            SbAssert.NotNull(name, "Name must not be null!");
            SbAssert.NotNull(key, "Key must not be null!");

            return Execute(name, database => database.StringGet(key));
        }

        public void Put(string name, byte[] key, byte[] value, TimeSpan ttl)
        {
            SbAssert.NotNull(name, "Name must not be null!");
            SbAssert.NotNull(key, "Key must not be null!");
            SbAssert.NotNull(value, "Value must not be null!");

            Execute(name, database =>
            {
                if (ShouldExpireWithin(ttl))
                {
                    database.StringSet(key, value, ttl);
                }
                else
                {
                    database.StringSet(key, value);
                }
                return "ok";
            });
        }

        public byte[] PutIfAbsent(string name, byte[] key, byte[] value, TimeSpan ttl)
        {
            SbAssert.NotNull(name, "Name must not be null!");
            SbAssert.NotNull(key, "Key must not be null!");
            SbAssert.NotNull(value, "Value must not be null!");

            return Execute<byte[]>(name, database =>
            {
                if (IsLockingCacheWriter())
                {
                    DoLock(name, database);
                }

                try
                {
                    if (database.StringSet(key, value, when: When.NotExists))
                    {

                        if (ShouldExpireWithin(ttl))
                        {
                            database.KeyExpire(key, ttl);
                        }
                        return null;
                    }

                    return database.StringGet(key);
                }
                finally
                {

                    if (IsLockingCacheWriter())
                    {
                        DoUnlock(name, database);
                    }
                }
            });
        }

        public void Remove(string name, byte[] key)
        {
            SbAssert.NotNull(name, "Name must not be null!");
            SbAssert.NotNull(key, "Key must not be null!");

            Execute(name, database => database.KeyDelete(key));
        }

        public void Clean(string name, byte[] pattern)
        {
            SbAssert.NotNull(name, "Name must not be null!");

            Execute(name, database =>
            {
                bool wasLocked = false;

                try
                {

                    if (IsLockingCacheWriter())
                    {
                        DoLock(name, database);
                        wasLocked = true;
                    }

                    var endpoints = ConnectionMultiplexer.GetEndPoints(true);


                    foreach (var endpoint in endpoints)
                    {
                        var server = ConnectionMultiplexer.GetServer(endpoint);
                        var keys = server.Keys(pattern: pattern).ToArray();
                        database.KeyDelete(keys);
                    }
                }
                finally
                {

                    if (wasLocked && IsLockingCacheWriter())
                    {
                        DoUnlock(name, database);
                    }
                }

                return "OK";
            });
        }

        private T Execute<T>(string name, Func<IDatabase, T> callBack)
        {
            var dataBase = ConnectionMultiplexer.GetDatabase();
            try
            {
                CheckAndPotentiallyWaitUntilUnlocked(name, dataBase);
                return callBack(dataBase);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
            }
            finally
            {
               
            }
            return default;
        }

        /// <summary>
        /// 是否开启锁，判断标准就是sleepTime>0
        /// </summary>
        /// <returns></returns>
        private bool IsLockingCacheWriter()
        {
            return !SleepTime.IsZero() && !SleepTime.IsNegative();
        }

        private void ExecuteLockFree(Action<IDatabase> callBack)
        {

            var dataBase = ConnectionMultiplexer.GetDatabase();

            try
            {
                callBack(dataBase);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {

            }
        }

        /// <summary>
        /// 检查并可能等待解锁
        /// </summary>
        /// <param name="name"></param>
        /// <param name="database"></param>
        private void CheckAndPotentiallyWaitUntilUnlocked(string name, IDatabase database)
        {
            //没有开启锁定，即sleepTime为0,直接返回
            if (!IsLockingCacheWriter())
            {
                return;
            }

            //如果锁存在就会阻塞等待
            try
            {
                while (DoCheckLock(name, database))
                {
                    Thread.Sleep((int)SleepTime.TotalMilliseconds);
                }
            }
            catch (ThreadInterruptedException ex)
            {

                // Re-interrupt current thread, to allow other participants to react.
                Thread.CurrentThread.Interrupt();

                throw new Exception(string.Format("Interrupted while waiting to unlock cache %s", name),
                    ex);
            }
        }

        /// <summary>
        /// 判断是否有锁定时间
        /// </summary>
        /// <param name="ttl"></param>
        /// <returns></returns>
        private static bool ShouldExpireWithin(TimeSpan ttl)
        {
            return ttl != null && !ttl.IsZero() && !ttl.IsNegative();
        }

        /// <summary>
        /// 获得key的加锁表示
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static byte[] CreateCacheLockKey(string name)
        {
            return (name + "~lock").GetBytes();
        }

        /// <summary>
        /// Explicitly set a write lock on a cache
        /// 给缓存上锁
        /// </summary>
        /// <param name="name"></param>
        private void Lock(string name)
        {
            Execute(name, (database) => DoLock(name, database));
        }

        /// <summary>
        /// 给缓存解锁
        /// </summary>
        /// <param name="name"></param>
        private void Unlock(string name)
        {
            ExecuteLockFree(database => DoUnlock(name, database));
        }

        private bool DoLock(string name, IDatabase database)
        {
            return database.StringSet(CreateCacheLockKey(name), new byte[0], null, When.NotExists, CommandFlags.None);
        }

        private bool DoUnlock(string name, IDatabase database)
        {
            return database.KeyDelete(CreateCacheLockKey(name));
        }

        private bool DoCheckLock(string name, IDatabase database)
        {
            return database.KeyExists(CreateCacheLockKey(name));
        }


    }
}