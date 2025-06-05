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
using SummerBoot.Core;
using System.Security.Claims;

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
        /// Sets column name mapping at database unit scope;设置数据库单元范围内的列名映射
        /// </summary>
        public Func<string, string> ColumnNameMapping { get; set; }
        /// <summary>
        /// Set Table name mapping at database unit scope;设置数据库单元范围内的表名映射
        /// </summary>
        public Func<string, string> TableNameMapping { get; set; }
        public Dictionary<Type, DbType> ParameterTypeMaps { get; }
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

        #region events

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

        #endregion

        /// <summary>
        /// Query timeout;查询超时时间
        /// </summary>
        public int CommandTimeout { get; set; } = 3000;
        /// <summary>
        ///Database Generator Class;
        /// 数据库生成器类
        /// </summary>
        public Type IDbGeneratorType { get; private set; }
        /// <summary>
        ///Entity Class Handler
        ///实体类处理程序
        /// </summary>
        public Type EntityClassHandlerType { get; private set; }
        /// <summary>
        /// Database unit id;数据库单元id
        /// </summary>
        public Guid Id { private set; get; }

        public DatabaseUnit(Type iUnitOfWorkType, Type dbConnectionType, string connectionString)
        {
            this.IUnitOfWorkType = iUnitOfWorkType;
            this.DbConnectionType = dbConnectionType;
            this.ConnectionString = connectionString;
            this.ParameterTypeMaps = SbUtil.CsharpTypeToDbTypeMap.DeepClone();
            this.Id = Guid.NewGuid();
            TypeHandlers.TryAdd(Id, new Dictionary<Type, Type>());
        }

        /// <summary>
        /// Unit of Work Type;工作单元类型
        /// </summary>
        public Type IUnitOfWorkType { get; }
        /// <summary>
        /// Automatically generated list of storage types;自动生成的仓储类型列表
        /// </summary>
        public List<Type> BindRepositoryTypes { get; private set; } = new List<Type>();

        /// <summary>
        /// Manually generated list of storage types;手动生成的仓储类型列表
        /// </summary>
        //public Dictionary<Type, Type> BindManualRepositoryTypes { get; private set; } = new Dictionary<Type, Type>();

        /// <summary>
        /// Binding a single repository;绑定单个仓储
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        public void BindRepository<T, TEntity>() where T : IBaseRepository<TEntity> where TEntity : class
        {
            this.BindRepositoryTypes.Add(typeof(T));
        }

        /// <summary>
        /// Automatic storage interface through feature binding;通过特性绑定自动仓储接口
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        public void BindRepositoriesWithAttribute<TAttribute>() where TAttribute : AutoRepositoryAttribute
        {
            var types = SbUtil.GetAppAllTypes();
            var autoRepositoryTypes = types.Where(it => it.IsInterface && it.GetCustomAttribute<TAttribute>()?.GetType() == typeof(TAttribute)).ToList();

            foreach (var autoRepositoryType in autoRepositoryTypes)
            {
                this.BindRepository(autoRepositoryType);
            }
        }

        /// <summary>
        /// Manual repository interface through feature binding;通过特性绑定手动仓储接口
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        public void BindManualRepositoriesWithAttribute<TAttribute>() where TAttribute : ManualRepositoryAttribute
        {
            var types = SbUtil.GetAppAllTypes();
            var autoRepositoryTypes = types.Where(it => it.IsClass && it.GetCustomAttribute<TAttribute>()?.GetType() == typeof(TAttribute)).ToList();

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
        public void BindDbGeneratorType<T>() where T : IDbGenerator
        {
            this.IDbGeneratorType = typeof(T);
        }

        /// <summary>
        /// Binding entity class handler
        /// 绑定实体类处理程序
        /// </summary>
        /// <param name="entityClassHandlerType"></param>
        public void BindEntityClassHandlerType(Type entityClassHandlerType)
        {
            CheckEntityClassHandlerType(entityClassHandlerType);
            this.EntityClassHandlerType = entityClassHandlerType;
        }

        private void CheckEntityClassHandlerType(Type entityClassHandlerType)
        {
            if (!entityClassHandlerType.IsClass || !typeof(IEntityClassHandler).IsAssignableFrom(entityClassHandlerType))
            {
                throw new Exception("Entity class that needs to inherit from IEntityClassHandler");
            }
        }
        /// <summary>
        /// Binding entity class handler
        /// 绑定实体类处理程序
        /// </summary>
        public void BindEntityClassHandlerType<T>() where T : IEntityClassHandler
        {
            CheckEntityClassHandlerType(typeof(T));
            this.EntityClassHandlerType = typeof(T);
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
        public void SetParameterTypeMap(Type type, DbType dbType)
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
