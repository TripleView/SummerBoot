using SummerBoot.Repository.Core;
using SummerBoot.Repository.DataMigrate;
using SummerBoot.Repository.ExpressionParser.Parser;
using SummerBoot.Repository.Generator;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace SummerBoot.Repository
{
    public delegate void RepositoryEvent(object entity);

    public delegate void RepositoryLogEvent(DbQueryResult dbQueryResult);

    public delegate string RepositoryReplaceSqlEvent(string sql);
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
                [typeof(DateTime)] = DbType.DateTime,
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
                [typeof(DateTime?)] = DbType.DateTime,
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
                [typeof(DateTime)] = DbType.DateTime,
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
                [typeof(DateTime?)] = DbType.DateTime,
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
                [typeof(DateTime)] = DbType.DateTime,
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
                [typeof(DateTime?)] = DbType.DateTime,
                [typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
                [typeof(TimeSpan?)] = null,
                [typeof(object)] = DbType.Object
            }},
            {DatabaseType.Pgsql,new Dictionary<Type, DbType?>(37)
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
                [typeof(DateTime)] = DbType.DateTime,
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
                [typeof(DateTime?)] = DbType.DateTime,
                [typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
                [typeof(TimeSpan?)] = null,
                [typeof(object)] = DbType.Object
            }},
        };

        /// <summary>
        /// Sets column name mapping at database unit scope;设置数据库单元范围内的列名映射
        /// </summary>
        public Func<string, string> ColumnNameMapping { get; set; }
        /// <summary>
        /// Set Table name mapping at database unit scope;设置数据库单元范围内的表名映射
        /// </summary>
        public Func<string, string> TableNameMapping { get; set; }
        public Dictionary<Type, DbType?> ParameterTypeMaps { get; }
        /// <summary>
        /// Is it data migration mode? If yes, ignore id auto-increment when inserting data
        /// 是否为数据迁移模式,如果是，则插入数据时忽略id自增
        /// </summary>
        internal bool IsDataMigrateMode { get; private set; }

        internal Type DataMigrateRepositoryType { get; private set; }
        /// <summary>
        /// 添加数据迁移功能
        /// Add data migration function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddDataMigrate<T>() where T : IDataMigrateRepository
        {
            this.IsDataMigrateMode = true;
            DataMigrateRepositoryType = typeof(T);
        }

        public static ConcurrentDictionary<Guid, Dictionary<Type, Type>> TypeHandlers { get; } =
            new ConcurrentDictionary<Guid, Dictionary<Type, Type>>();
        /// <summary>
        /// 插入前事件
        /// </summary>
        public event RepositoryEvent BeforeInsert;
        /// <summary>
        ///log Event
        /// 打印事件
        /// </summary>
        public event RepositoryLogEvent LogSql;
        /// <summary>
        /// 更新前事件
        /// </summary>
        public event RepositoryEvent BeforeUpdate;

        /// <summary>
        /// ReplaceSql event
        /// 允许替换sql事件
        /// </summary>
        public event RepositoryReplaceSqlEvent ReplaceSql;

        public string OnReplaceSql(string sql)
        {
            if (ReplaceSql != null)
            {
                return ReplaceSql(sql);
            }

            return sql;
        }

        public void OnLogSqlInfo(DbQueryResult dbQueryResult)
        {
            if (LogSql != null)
            {
                LogSql(dbQueryResult);
            }
        }

        public void OnBeforeInsert(object entity)
        {
            if (BeforeInsert != null)
            {
                BeforeInsert(entity);
            }
        }

        public void OnBeforeUpdate(object entity)
        {
            if (BeforeUpdate != null)
            {
                BeforeUpdate(entity);
            }

        }
        /// <summary>
        /// 查询超时时间
        /// </summary>
        public int CommandTimeout { get; set; } = 3000;
        /// <summary>
        /// 数据库生成器类
        /// </summary>
        public Type IDbGeneratorType { get; set; }
        /// <summary>
        /// 数据库单元id
        /// </summary>
        public Guid Id { private set; get; }

        public DatabaseUnit(Type iUnitOfWorkType, Type dbConnectionType, string connectionString)
        {
            this.IUnitOfWorkType = iUnitOfWorkType;
            this.DbConnectionType = dbConnectionType;
            this.ConnectionString = connectionString;
            this.ParameterTypeMaps = AllDatabaseParameterTypeMaps[DatabaseType];
            this.Id = Guid.NewGuid();
            TypeHandlers.TryAdd(Id, new Dictionary<Type, Type>());
        }
        public Type IUnitOfWorkType { get; }
        public List<Type> BindRepositoryTypes { get; private set; } = new List<Type>();

        /// <summary>
        /// 绑定单个仓储
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        public void BindRepository<T, TEntity>() where T : IBaseRepository<TEntity> where TEntity : class
        {
            this.BindRepositoryTypes.Add(typeof(T));
        }

        /// <summary>
        /// 通过特性绑定仓储接口
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        public void BindRepositorysWithAttribute<TAttribute>() where TAttribute : AutoRepositoryAttribute
        {
            var autoRepositoryTypes = new List<Type>();
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(it => !it.IsDynamic).ToList();
            var tName = typeof(TAttribute).Name;
            foreach (var assembly in allAssemblies)
            {
                var types = assembly.GetExportedTypes().Where(it => it.IsInterface && it.GetCustomAttribute<TAttribute>()?.GetType() ==typeof(TAttribute)).ToList();
                if (types.Any())
                {
                    autoRepositoryTypes.AddRange(types);
                }
               
            }
            foreach (var autoRepositoryType in autoRepositoryTypes)
            {
                this.BindRepository(autoRepositoryType);
            }
        }

        /// <summary>
        /// 绑定数据库生成器
        /// </summary>
        /// <param name="iDbGeneratorType"></param>
        public void BindDbGeneratorType(Type iDbGeneratorType)
        {
            this.IDbGeneratorType = iDbGeneratorType;
        }

        /// <summary>
        /// 绑定数据库生成器
        /// </summary>
        /// <param name="iDbGeneratorType"></param>
        public void BindDbGeneratorType<T>() where T : IDbGenerator
        {
            this.IDbGeneratorType = typeof(T);
        }

        public void BindRepository(Type iRepositoryType)
        {
            this.BindRepositoryTypes.Add(iRepositoryType);
        }
        /// <summary>
        /// 设置参数类型映射
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dbType"></param>
        public void SetParameterTypeMap(Type type, DbType? dbType)
        {
            if (this.ParameterTypeMaps.ContainsKey(type))
            {
                this.ParameterTypeMaps[type] = dbType;

                //添加可空和非空类型
                var nullableUnderlyingType = Nullable.GetUnderlyingType(type);
                if (nullableUnderlyingType == null)
                {
                    nullableUnderlyingType = typeof(Nullable<>).MakeGenericType(type);
                }
                this.ParameterTypeMaps[nullableUnderlyingType] = dbType;
            }
            else
            {
                this.ParameterTypeMaps.Add(type, dbType);
                //添加可空和非空类型
                var nullableUnderlyingType = Nullable.GetUnderlyingType(type);
                if (nullableUnderlyingType == null)
                {
                    nullableUnderlyingType = typeof(Nullable<>).MakeGenericType(type);
                }
                this.ParameterTypeMaps.Add(nullableUnderlyingType, dbType);
            }
        }

        /// <summary>
        /// 设置自定义的将值转换为数据库参数和将数据库返回的值解析为目标值的映射关系
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="t"></param>
        public void SetTypeHandler(Type type, ITypeHandler t)
        {
            this.InternalSetTypeHandler(type, t);
            //return;
            //添加可空和非空类型
            var nullableUnderlyingType = Nullable.GetUnderlyingType(type);
            if (nullableUnderlyingType != null)
            {
                this.InternalSetTypeHandler(nullableUnderlyingType, t);
            }
            else
            {
                nullableUnderlyingType = typeof(Nullable<>).MakeGenericType(type);
                this.InternalSetTypeHandler(nullableUnderlyingType, t);
            }
        }

        private void InternalSetTypeHandler(Type type, ITypeHandler t)
        {
            var typeHandlerCacheType = DatabaseContext.GenerateTypeHandlerCacheClass(type);
            typeHandlerCacheType.GetMethod("SetHandler").Invoke(null, new object[] { t });
            TypeHandlers[this.Id][type] = typeHandlerCacheType;
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

        public bool IsPgsql
        {
            get
            {
                var dbName = DbConnectionType.FullName;
                return dbName.ToLower().IndexOf("pgsql") > -1;
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

                if (IsPgsql)
                {
                    databaseType = DatabaseType.Pgsql;
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
