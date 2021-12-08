using System;
using System.Data;

namespace SummerBoot.Repository
{
    public interface IDbFactory : IDisposable
    {
        IDbTransaction GetDbTransaction();
        IDbConnection GetDbConnection();
        /// <summary>
        /// 释放资源
        /// </summary>
        void ReleaseResources();
    }
}