using Microsoft.AspNetCore.Http;
using System;
using SummerBoot.Repository;

namespace SummerBoot.Core
{
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// 引用次数，开启一次事物加+1,当次数为1时提交，主要是为了防止事物嵌套
        /// </summary>
        int ActiveNumber { get; set; }

        /// <summary>
        /// 开启事务
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// 提交
        /// </summary>
        void Commit();

        /// <summary>
        /// 事物回滚
        /// </summary>
        void RollBack();

        /// <summary>
        /// 注册回调接口
        /// </summary>
        /// <param name="action"></param>
        void RegisterCallBack(Action action);

        IDbFactory DbFactory { get; }

        IEntityClassHandler EntityClassHandler { get; }
    }

    public interface IUnitOfWork1: IUnitOfWork{}
    public interface IUnitOfWork2 : IUnitOfWork { }
    public interface IUnitOfWork3 : IUnitOfWork { }
    public interface IUnitOfWork4 : IUnitOfWork { }
    public interface IUnitOfWork5 : IUnitOfWork { }
    public interface IUnitOfWork6 : IUnitOfWork { }

    public interface IUnitOfWork7 : IUnitOfWork { }
    public interface IUnitOfWork8 : IUnitOfWork { }

    public interface IUnitOfWork9 : IUnitOfWork { }
}