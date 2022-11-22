using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using StackExchange.Redis;
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
                if (!typeof(IDbConnection).IsAssignableFrom(value)) throw new Exception("DbConnectionType must be implement IDbConection");
                dbConnectionType = value;
            }
        }
        public string ConnectionString
        {
            set
            {
                if (value.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(value));
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
                return dbName.ToLower().IndexOf("sqlconnection") > -1 && dbName.ToLower().IndexOf("microsoft") > -1
                       || (dbName.ToLower().IndexOf("sqlconnection") > -1 && dbName.ToLower().IndexOf("system") > -1);
            }
        }
        public bool IsOracle
        {
            get
            {
                var dbName = DbConnectionType.FullName;
                return dbName.ToLower().IndexOf("oracle") > -1;
            }
        }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public DatabaseType DatabaseType
        {
            get
            {
                DatabaseType databaseType = DatabaseType.SqlServer;
                if (IsMysql)
                {
                    databaseType = DatabaseType.Mysql;
                }

                if (IsOracle)
                {
                    databaseType = DatabaseType.Oracle;
                }


                if (IsSqlServer)
                {
                    databaseType = DatabaseType.SqlServer;
                }

                if (IsSqlite)
                {
                    databaseType = DatabaseType.Sqlite;
                }

                return databaseType;
            }
        }
        /// <summary>
        /// sql语句里的参数标识符
        /// </summary>
        public static List<char> ParameterIdentifiers = new List<char>() { '@', ':', '?' };


        /// <summary>
        /// 数据库与实体类自动转换配置，对单表单字段进行特殊配置
        /// </summary>
        private Dictionary<string, Dictionary<string, string>> generatorMap = new Dictionary<string, Dictionary<string, string>>();


        /// <summary>
        /// 数据库与实体类自动转换配置，创建自定义字段映射，对单表单字段进行配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selector"></param>
        /// <param name="databaseMappingType"></param>
        private void GeneratorMap<T>(Expression<Func<T, object>> selector, string databaseMappingType)
        {
            if (selector.Body is MemberExpression memberExpression)
            {
                var memberInfo = memberExpression.Member;
                var columnAttribute = memberInfo.GetCustomAttribute<ColumnAttribute>();

                var columnName = columnAttribute?.Name ?? memberInfo.Name;
                var tableType = memberExpression.Member.DeclaringType;
                if (tableType == null)
                {
                    return;
                }
                var tableAttribute = tableType.GetCustomAttribute<TableAttribute>();
                var tableName = tableAttribute?.Name ?? tableType.Name;
                if (generatorMap.ContainsKey(tableName))
                {
                    var tableFieldMaps = generatorMap[tableName];
                    tableFieldMaps[columnName] = databaseMappingType;
                }
                else
                {
                    generatorMap[tableName] = new Dictionary<string, string>()
                    {
                        {columnName, databaseMappingType}
                    };
                }
            }
            else
            {
                throw new NotSupportedException("not support this selector");
            }
        }
        /// <summary>
        /// 数据库连接字符串
        /// </summary>

        /// <summary>
        /// 全局唯一实例
        /// </summary>
        public static RepositoryOption Instance { set; get; }

        /// <summary>
        /// 插入的时候自动添加创建时间，数据库实体类必须继承于BaseEntity
        /// </summary>
        public bool AutoAddCreateOn { get; set; } = false;

        /// <summary>
        /// 插入的时候自动添加创建时间，使用utc时间
        /// </summary>
        public bool AutoAddCreateOnUseUtc { get; set; } = false;

        /// <summary>
        /// update的时候自动更新最后更新时间字段，数据库实体类必须继承于BaseEntity
        /// </summary>
        public bool AutoUpdateLastUpdateOn { get; set; } = false;
        /// <summary>
        /// update的时候自动更新最后更新时间字段，使用utc时间
        /// </summary>
        public bool AutoUpdateLastUpdateOnUseUtc { get; set; } = false;
        /// <summary>
        /// 是否使用软删除
        /// </summary>
        public bool IsUseSoftDelete { get; set; }

        public Dictionary<string, DatabaseUnit> DatabaseUnits { get; } = new Dictionary<string, DatabaseUnit>();

        public void AddDatabaseUnit<TDbConnection, TUnitOfWork>(string connectionString, Action<DatabaseUnit> optionAction) where TDbConnection : IDbConnection where TUnitOfWork : IUnitOfWork
        {
            if (connectionString.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("ConnectionString can not be null");
            }

            var databaseUnit = new DatabaseUnit(typeof(TUnitOfWork), typeof(TDbConnection), connectionString);

            optionAction(databaseUnit);

            DatabaseUnits[databaseUnit.ConnectionString] = databaseUnit;
        }
    }


    /// <summary>
    /// 数据库单元
    /// </summary>
    public class DatabaseUnit
    {
        /// <summary>
        /// 所有数据库的参数与数据库类型的映射汇总
        /// </summary>
        private readonly Dictionary<DatabaseType, Dictionary<Type, DbType?>> AllDatabaseParameterTypeMaps = new Dictionary<DatabaseType, Dictionary<Type, DbType?>>()
        {
            {DatabaseType.Mysql,new Dictionary<Type, DbType?>(37)
            {
                [typeof(byte)] = DbType.Byte,
                [typeof(sbyte)] = DbType.SByte,
                [typeof(short)] = DbType.Int16,
                [typeof(ushort)] = DbType.UInt16,
                [typeof(int)] = DbType.Int32,
                [typeof(uint)] = DbType.UInt32,
                [typeof(long)] = DbType.Int64,
                [typeof(ulong)] = DbType.UInt64,
                [typeof(float)] = DbType.Single,
                [typeof(double)] = DbType.Double,
                [typeof(decimal)] = DbType.Decimal,
                [typeof(bool)] = DbType.Boolean,
                [typeof(string)] = DbType.String,
                [typeof(char)] = DbType.StringFixedLength,
                [typeof(Guid)] = DbType.Guid,
                [typeof(DateTime)] = null,
                [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
                [typeof(TimeSpan)] = null,
                [typeof(byte[])] = DbType.Binary,
                [typeof(byte?)] = DbType.Byte,
                [typeof(sbyte?)] = DbType.SByte,
                [typeof(short?)] = DbType.Int16,
                [typeof(ushort?)] = DbType.UInt16,
                [typeof(int?)] = DbType.Int32,
                [typeof(uint?)] = DbType.UInt32,
                [typeof(long?)] = DbType.Int64,
                [typeof(ulong?)] = DbType.UInt64,
                [typeof(float?)] = DbType.Single,
                [typeof(double?)] = DbType.Double,
                [typeof(decimal?)] = DbType.Decimal,
                [typeof(bool?)] = DbType.Boolean,
                [typeof(char?)] = DbType.StringFixedLength,
                [typeof(Guid?)] = DbType.Guid,
                [typeof(DateTime?)] = null,
                [typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
                [typeof(TimeSpan?)] = null,
                [typeof(object)] = DbType.Object
            }},
            {DatabaseType.SqlServer,new Dictionary<Type, DbType?>(37)
            {
                [typeof(byte)] = DbType.Byte,
                [typeof(sbyte)] = DbType.SByte,
                [typeof(short)] = DbType.Int16,
                [typeof(ushort)] = DbType.UInt16,
                [typeof(int)] = DbType.Int32,
                [typeof(uint)] = DbType.UInt32,
                [typeof(long)] = DbType.Int64,
                [typeof(ulong)] = DbType.UInt64,
                [typeof(float)] = DbType.Single,
                [typeof(double)] = DbType.Double,
                [typeof(decimal)] = DbType.Decimal,
                [typeof(bool)] = DbType.Boolean,
                [typeof(string)] = DbType.String,
                [typeof(char)] = DbType.StringFixedLength,
                [typeof(Guid)] = DbType.Guid,
                [typeof(DateTime)] = null,
                [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
                [typeof(TimeSpan)] = null,
                [typeof(byte[])] = DbType.Binary,
                [typeof(byte?)] = DbType.Byte,
                [typeof(sbyte?)] = DbType.SByte,
                [typeof(short?)] = DbType.Int16,
                [typeof(ushort?)] = DbType.UInt16,
                [typeof(int?)] = DbType.Int32,
                [typeof(uint?)] = DbType.UInt32,
                [typeof(long?)] = DbType.Int64,
                [typeof(ulong?)] = DbType.UInt64,
                [typeof(float?)] = DbType.Single,
                [typeof(double?)] = DbType.Double,
                [typeof(decimal?)] = DbType.Decimal,
                [typeof(bool?)] = DbType.Boolean,
                [typeof(char?)] = DbType.StringFixedLength,
                [typeof(Guid?)] = DbType.Guid,
                [typeof(DateTime?)] = null,
                [typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
                [typeof(TimeSpan?)] = null,
                [typeof(object)] = DbType.Object
            }},
            {DatabaseType.Oracle,new Dictionary<Type, DbType?>(37)
            {
                [typeof(byte)] = DbType.Byte,
                [typeof(sbyte)] = DbType.SByte,
                [typeof(short)] = DbType.Int16,
                [typeof(ushort)] = DbType.UInt16,
                [typeof(int)] = DbType.Int32,
                [typeof(uint)] = DbType.UInt32,
                [typeof(long)] = DbType.Int64,
                [typeof(ulong)] = DbType.UInt64,
                [typeof(float)] = DbType.Single,
                [typeof(double)] = DbType.Double,
                [typeof(decimal)] = DbType.Decimal,
                [typeof(bool)] = DbType.Boolean,
                [typeof(string)] = DbType.String,
                [typeof(char)] = DbType.StringFixedLength,
                [typeof(Guid)] = DbType.Guid,
                [typeof(DateTime)] = null,
                [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
                [typeof(TimeSpan)] = null,
                [typeof(byte[])] = DbType.Binary,
                [typeof(byte?)] = DbType.Byte,
                [typeof(sbyte?)] = DbType.SByte,
                [typeof(short?)] = DbType.Int16,
                [typeof(ushort?)] = DbType.UInt16,
                [typeof(int?)] = DbType.Int32,
                [typeof(uint?)] = DbType.UInt32,
                [typeof(long?)] = DbType.Int64,
                [typeof(ulong?)] = DbType.UInt64,
                [typeof(float?)] = DbType.Single,
                [typeof(double?)] = DbType.Double,
                [typeof(decimal?)] = DbType.Decimal,
                [typeof(bool?)] = DbType.Boolean,
                [typeof(char?)] = DbType.StringFixedLength,
                [typeof(Guid?)] = DbType.Guid,
                [typeof(DateTime?)] = null,
                [typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
                [typeof(TimeSpan?)] = null,
                [typeof(object)] = DbType.Object
            }},
            {DatabaseType.Sqlite,new Dictionary<Type, DbType?>(37)
            {
                [typeof(byte)] = DbType.Byte,
                [typeof(sbyte)] = DbType.SByte,
                [typeof(short)] = DbType.Int16,
                [typeof(ushort)] = DbType.UInt16,
                [typeof(int)] = DbType.Int32,
                [typeof(uint)] = DbType.UInt32,
                [typeof(long)] = DbType.Int64,
                [typeof(ulong)] = DbType.UInt64,
                [typeof(float)] = DbType.Single,
                [typeof(double)] = DbType.Double,
                [typeof(decimal)] = DbType.Decimal,
                [typeof(bool)] = DbType.Boolean,
                [typeof(string)] = DbType.String,
                [typeof(char)] = DbType.StringFixedLength,
                [typeof(Guid)] = DbType.Guid,
                [typeof(DateTime)] = null,
                [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
                [typeof(TimeSpan)] = null,
                [typeof(byte[])] = DbType.Binary,
                [typeof(byte?)] = DbType.Byte,
                [typeof(sbyte?)] = DbType.SByte,
                [typeof(short?)] = DbType.Int16,
                [typeof(ushort?)] = DbType.UInt16,
                [typeof(int?)] = DbType.Int32,
                [typeof(uint?)] = DbType.UInt32,
                [typeof(long?)] = DbType.Int64,
                [typeof(ulong?)] = DbType.UInt64,
                [typeof(float?)] = DbType.Single,
                [typeof(double?)] = DbType.Double,
                [typeof(decimal?)] = DbType.Decimal,
                [typeof(bool?)] = DbType.Boolean,
                [typeof(char?)] = DbType.StringFixedLength,
                [typeof(Guid?)] = DbType.Guid,
                [typeof(DateTime?)] = null,
                [typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
                [typeof(TimeSpan?)] = null,
                [typeof(object)] = DbType.Object
            }}
        };

        public Dictionary<Type, DbType?> DefaultParameterTypeMaps { get; }

        public DatabaseUnit(Type iUnitOfWorkType, Type dbConnectionType, string connectionString)
        {
            this.IUnitOfWorkType = iUnitOfWorkType;
            this.DbConnectionType = dbConnectionType;
            this.ConnectionString = connectionString;
            this.DefaultParameterTypeMaps = AllDatabaseParameterTypeMaps[DatabaseType];
        }
        public Type IUnitOfWorkType { get; }
        public List<Type> BindRepositoryTypes { get; private set; } = new List<Type>();

        public void Bind<T, TEntity>() where T : IBaseRepository<TEntity> where TEntity : class
        {
            this.BindRepositoryTypes.Add(typeof(T));
        }

        /// <summary>
        /// 设置类型映射
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dbType"></param>
        public void SetTypeMap(Type type,DbType? dbType)
        {
            if (this.DefaultParameterTypeMaps.ContainsKey(type))
            {
                this.DefaultParameterTypeMaps[type] = dbType;
            }
            else
            {
                this.DefaultParameterTypeMaps.Add(type,dbType);
            }
        }

        /// <summary>
        /// 数据库连接器类型
        /// </summary>
        public Type DbConnectionType
        {
            get;
        }
        public string ConnectionString
        {
            get;
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
                return dbName.ToLower().IndexOf("sqlconnection") > -1 && dbName.ToLower().IndexOf("microsoft") > -1
                       || (dbName.ToLower().IndexOf("sqlconnection") > -1 && dbName.ToLower().IndexOf("system") > -1);
            }
        }
        public bool IsOracle
        {
            get
            {
                var dbName = DbConnectionType.FullName;
                return dbName.ToLower().IndexOf("oracle") > -1;
            }
        }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public DatabaseType DatabaseType
        {
            get
            {
                DatabaseType databaseType = DatabaseType.SqlServer;
                if (IsMysql)
                {
                    databaseType = DatabaseType.Mysql;
                }

                if (IsOracle)
                {
                    databaseType = DatabaseType.Oracle;
                }


                if (IsSqlServer)
                {
                    databaseType = DatabaseType.SqlServer;
                }

                if (IsSqlite)
                {
                    databaseType = DatabaseType.Sqlite;
                }

                return databaseType;
            }
        }
    }
}