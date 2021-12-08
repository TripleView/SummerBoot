using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SqlOnline.Utils;
using SummerBoot.Repository;

namespace SummerBoot.Core
{
    public class UnitOfWork : IUnitOfWork
    {
        public IDbFactory DbFactory { get; }
        public UnitOfWork(ILogger<UnitOfWork> logger, IDbFactory dbFactory)
        {
            this.Logger = logger;
            DbFactory = dbFactory;
        }
        /// <summary>
        /// 回调事件
        /// </summary>
        private readonly IList<Action> _callBackActions = new List<Action>();

        /// <summary>
        /// 工作单元引用次数，当次数为0时提交，主要为了防止事物嵌套
        /// </summary>
        public int ActiveNumber { get; set; } = 0;

        private ILogger<UnitOfWork> Logger { set; get; }

        /// <summary>
        /// 是否开启工作单元
        /// </summary>
        private bool EnableUnitOfWork => DbFactory != null;

        public void BeginTransaction()
        {
            if (this.ActiveNumber == 0 && EnableUnitOfWork)
            {
                DbFactory.GetDbTransaction();

                Logger.LogDebug("开启事务");
            }
            this.ActiveNumber++;
        }


        public void Commit()
        {
            this.ActiveNumber--;
            if (this.ActiveNumber == 0 && EnableUnitOfWork)
            {
                var isCommitSuccess = false;
                try
                {
                    DbFactory.GetDbTransaction().Commit();
                    isCommitSuccess = true;
                }
                catch (Exception e)
                {
                    DbFactory.GetDbTransaction().Rollback();
                    isCommitSuccess = false;
                    Logger.LogError("提交事务失败", e);
                }
                finally
                {
                    DbFactory.ReleaseResources();
                    if (isCommitSuccess && this._callBackActions != null)
                    {
                        foreach (Action callBackAction in this._callBackActions)
                        {
                            callBackAction();
                        }
                    }
                    Logger.LogDebug("提交事务");
                }
            }
        }

        public void Dispose()
        {
            this._callBackActions.Clear();
        }

        public void RegisterCallBack(Action action)
        {
            _callBackActions.Add(action);
        }

        public void RollBack()
        {
            this.ActiveNumber--;
            if (this.ActiveNumber == 0 && EnableUnitOfWork)
            {

                try
                {
                    DbFactory.GetDbTransaction().Rollback();
                    Logger.LogDebug("回滚事务");
                }
                catch (Exception e)
                {
                    Logger.LogError("回滚事务失败", e);
                }
                finally
                {
                    DbFactory.ReleaseResources();
                }
            }
        }
    }
}