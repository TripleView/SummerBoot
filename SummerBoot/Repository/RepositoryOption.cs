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

        public bool IsSqlite
        {
            get
            {
                var dbName = DbConnectionType.FullName;
                return dbName.ToLower().IndexOf("sqlite") > -1;
            }
        }
        public bool IsMysql
        {
            get
            {
                var dbName = DbConnectionType.FullName;
                return dbName.ToLower().IndexOf("mysql") > -1;
            }
        }

        public bool IsSqlServer
        {
            get
            {
                var dbName = DbConnectionType.FullName;
                return dbName.ToLower().IndexOf("microsoft") > -1|| dbName.ToLower().IndexOf("system") > -1;
            }
        }
        public bool IsOracle
        {
            get
            {
                var dbName = DbConnectionType.FullName;
                return dbName.ToLower().IndexOf("oracle") > -1 ;
            }
        }
    }
}