using System;
using System.Data;
using SummerBoot.Core;

namespace SummerBoot.Repository
{
    public class RepositoryOption
    {
        private Type dbConnectionType;

        private string connectionString;

        /// <summary>
        /// 数据库连接器类型
        /// </summary>
        public Type DbConnectionType
        {
            get => dbConnectionType;
            set
            {
                if(!typeof(IDbConnection).IsAssignableFrom(value)) throw new Exception("DbConnectionType must be implement IDbConection");
                dbConnectionType = value;
            }
        }

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnectionString {
            set
            {
                if(value.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(value));
                connectionString = value;
            }
            get => connectionString;
        }
    }
}