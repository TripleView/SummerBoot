using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SqlOnline.Utils;
using SummerBoot.Repository;

namespace SummerBoot.Core
{
    public class UnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// 回调事件
        /// </summary>
        private readonly IList<Action> callBackActions = new List<Action>();

        /// <summary>
        /// 工作单元引用次数，当次数为0时提交，主要为了防止事物嵌套
        /// </summary>
        public int ActiveNumber { get; set; } = 0;

        [Autowired]
        private IDbFactory DbFactory { set; get; }

        private ILogger<UnitOfWork> Logger { set; get; }

        /// <summary>
        /// 是否开启工作单元
        /// </summary>
        private bool EnableUnitOfWork => DbFactory != null;

        public UnitOfWork(ILogger<UnitOfWork> logger)
        {
            this.Logger = logger;
        }


        public void BeginTransaction()
        {
            if (this.ActiveNumber == 0 && EnableUnitOfWork)
            {
                DbFactory.BeginTransaction();

                Logger.LogDebug("开启事务");
            }
            this.ActiveNumber++;
        }

        public void Commit()
        {
            this.ActiveNumber--;
            if (this.ActiveNumber == 0)
            {
                if (EnableUnitOfWork && DbFactory.LongDbConnection != null)
                {
                    var isCommitSuccess = false;
                    try
                    {
                        DbFactory.LongDbTransaction.Commit();
                        isCommitSuccess = true;
                    }
                    catch (Exception e)
                    {
                        DbFactory.LongDbTransaction.Rollback();
                        isCommitSuccess = false;
                        Logger.LogError(e.ToString());
                    }
                    finally
                    {
                        if (isCommitSuccess && this.callBackActions != null)
                        {
                            foreach (Action callBackAction in this.callBackActions)
                            {
                                callBackAction();
                            }
                        }
                        this.Dispose();
                    }
                }

                Logger.LogDebug("提交事务");
            }

        }

        public void Dispose()
        {
            this.callBackActions.Clear();

            if(EnableUnitOfWork)DbFactory.Dispose();
        }

        public void RegisterCallBack(Action action)
        {
            callBackActions.Add(action);
        }

        public void RollBack()
        {
            if (this.ActiveNumber > 0 && DbFactory.LongDbTransaction != null && EnableUnitOfWork)
            {
                try
                {
                    DbFactory.LongDbTransaction.Rollback();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            Logger.LogDebug("回滚事务");
        }
    }
}