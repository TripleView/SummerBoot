using System;
using System.Data;
using SummerBoot.Core;

namespace SummerBoot.Repository
{
    public interface IDbFactory : IDisposable
    {
        public DatabaseUnit DatabaseUnit { get; }
        IDbTransaction GetDbTransaction();
        IDbConnection GetDbConnection();
        /// <summary>
        /// 释放资源
        /// </summary>
        void ReleaseResources();
    }
}