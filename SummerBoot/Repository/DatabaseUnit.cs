using SummerBoot.Repository.Core;
using SummerBoot.Repository.DataMigrate;
using SummerBoot.Repository.ExpressionParser.Parser;
using SummerBoot.Repository.Generator;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
    /// ���ݿⵥԪ
    /// </summary>
    public class DatabaseUnit
    {
        /// <summary>
        /// Sets column name mapping at database unit scope;�������ݿⵥԪ��Χ�ڵ�����ӳ��
        /// </summary>
        public Func<string, string> ColumnNameMapping { get; set; }
        /// <summary>
        /// Set Table name mapping at database unit scope;�������ݿⵥԪ��Χ�ڵı���ӳ��
        /// </summary>
        public Func<string, string> TableNameMapping { get; set; }
        public Dictionary<Type, DbType> ParameterTypeMaps { get; }
        /// <summary>
        /// Is it data migration mode? If yes, ignore id auto-increment when inserting data
        /// �Ƿ�Ϊ����Ǩ��ģʽ,����ǣ����������ʱ����id����
        /// </summary>
        internal bool IsDataMigrateMode { get; private set; }

        internal Type DataMigrateRepositoryType { get; private set; }
        /// <summary>
        /// �������Ǩ�ƹ���
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
        /// ����ǰ�¼�
        /// </summary>
        public event RepositoryEvent BeforeInsert;
        /// <summary>
        ///log Event
        /// ��ӡ�¼�
        /// </summary>
        public event RepositoryLogEvent LogSql;
        /// <summary>
        /// ����ǰ�¼�
        /// </summary>
        public event RepositoryEvent BeforeUpdate;

        /// <summary>
        /// ReplaceSql event
        /// �����滻sql�¼�
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
        /// Query timeout;��ѯ��ʱʱ��
        /// </summary>
        public int CommandTimeout { get; set; } = 3000;
        /// <summary>
        ///Database Generator Class;
        /// ���ݿ���������
        /// </summary>
        public Type IDbGeneratorType { get; private set; }
        /// <summary>
        ///Entity Class Handler
        ///ʵ���ദ�����
        /// </summary>
        public Type EntityClassHandlerType { get; private set; }
        /// <summary>
        /// Database unit id;���ݿⵥԪid
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
        /// Unit of Work Type;������Ԫ����
        /// </summary>
        public Type IUnitOfWorkType { get; }
        /// <summary>
        /// Automatically generated list of storage types;�Զ����ɵĲִ������б�
        /// </summary>
        public List<Type> BindRepositoryTypes { get; private set; } = new List<Type>();

        /// <summary>
        /// Manually generated list of storage types;�ֶ����ɵĲִ������б�
        /// </summary>
        //public Dictionary<Type, Type> BindManualRepositoryTypes { get; private set; } = new Dictionary<Type, Type>();

        /// <summary>
        /// Binding a single repository;�󶨵����ִ�
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        public void BindRepository<T, TEntity>() where T : IBaseRepository<TEntity> where TEntity : class
        {
            this.BindRepositoryTypes.Add(typeof(T));
        }

        /// <summary>
        /// Automatic storage interface through feature binding;ͨ�����԰��Զ��ִ��ӿ�
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
        /// Manual repository interface through feature binding;ͨ�����԰��ֶ��ִ��ӿ�
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
        /// �����ݿ�������
        /// </summary>
        /// <param name="iDbGeneratorType"></param>
        public void BindDbGeneratorType(Type iDbGeneratorType)
        {
            this.IDbGeneratorType = iDbGeneratorType;
        }

        /// <summary>
        /// �����ݿ�������
        /// </summary>
        public void BindDbGeneratorType<T>() where T : IDbGenerator
        {
            this.IDbGeneratorType = typeof(T);
        }

        /// <summary>
        /// Binding entity class handler
        /// ��ʵ���ദ�����
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
        /// ��ʵ���ദ�����
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
        /// ���ò�������ӳ��
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dbType"></param>
        public void SetParameterTypeMap(Type type, DbType dbType)
        {
            if (this.ParameterTypeMaps.ContainsKey(type))
            {
                this.ParameterTypeMaps[type] = dbType;

                //��ӿɿպͷǿ�����
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
                //��ӿɿպͷǿ�����
                var nullableUnderlyingType = Nullable.GetUnderlyingType(type);
                if (nullableUnderlyingType == null)
                {
                    nullableUnderlyingType = typeof(Nullable<>).MakeGenericType(type);
                }
                this.ParameterTypeMaps.Add(nullableUnderlyingType, dbType);
            }
        }

        /// <summary>
        /// �����Զ���Ľ�ֵת��Ϊ���ݿ�����ͽ����ݿⷵ�ص�ֵ����ΪĿ��ֵ��ӳ���ϵ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="t"></param>
        public void SetTypeHandler(Type type, ITypeHandler t)
        {
            this.InternalSetTypeHandler(type, t);
            //return;
            //��ӿɿպͷǿ�����
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
        /// ���ݿ�����������
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

        private int? sqlServerVersion;

        public int SqlServerVersion
        {
            get
            {
                if (sqlServerVersion.HasValue)
                {
                    return sqlServerVersion.Value;
                }

                var dbConnection = (DbConnection)DbConnectionType.CreateInstance(new object[] { this.ConnectionString });
                dbConnection.Open();
                var version = dbConnection.QueryFirstOrDefault<int>(this,
                    "  select CAST(PARSENAME(CAST(SERVERPROPERTY('ProductVersion') AS NVARCHAR(128)), 4) AS INT)");
                dbConnection.Close();
                this.sqlServerVersion = version;
                return version;
            }
            set
            {
                this.sqlServerVersion = value;
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
        /// ���ݿ�����
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
