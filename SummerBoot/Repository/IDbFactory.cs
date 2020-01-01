using System;
using System.Data;

namespace SummerBoot.Repository
{
    public interface IDbFactory:IDisposable
    {
        /// <summary>
        /// 长链接
        /// </summary>
        IDbConnection LongDbConnection { get; }

        /// <summary>
        /// 长链接的事物
        /// </summary>
        IDbTransaction LongDbTransaction { get; }

        /// <summary>
        /// 短链接
        /// </summary>
        IDbConnection ShortDbConnection { get; }

        /// <summary>
        /// 开启事务
        /// </summary>
        void BeginTransaction();
    }
}