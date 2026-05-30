using SummerBoot.Core;
using SummerBoot.Repository.Core;
using SummerBoot.Repository.DataMigrate;
using SummerBoot.Repository.ExpressionParser;
using SummerBoot.Repository.ExpressionParser.Parser;
using SummerBoot.Repository.Generator;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using SqlParser.Net.Ast.Expression;

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
        /// Mapping from C# types to database type names;c#类型到数据库类型名称的映射
        /// </summary>
        public Dictionary<Type, string> CsharpTypeToDatabaseTypeNameMaps { get; }
        /// <summary>
        /// Mapping from Database Type Names to C# Type Names;数据库类型名称到c#类型名称的映射
        /// </summary>
        public Dictionary<string, string> DatabaseTypeNameToCsharpTypeNameMaps { get; }
        /// <summary>
        /// Is it data migration mode? If yes, ignore id auto-increment when inserting data
        /// 是否为数据迁移模式,如果是，则插入数据时忽略id自增
        /// </summary>
        internal bool IsDataMigrateMode { get; private set; }

        internal Type DataMigrateRepositoryType { get; private set; }

        /// <summary>
        /// 数据库函数映射
        /// </summary>
        public Dictionary<MethodInfo, Func<FunctionCallInfo, SqlExpression>> SqlFunctionMappings { get; set; } =
            new Dictionary<MethodInfo, Func<FunctionCallInfo, SqlExpression>>();
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

        public Action<string, DynamicParameters> DebugSqlAction { set; get; }

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

        public void AddSqlFunctionMapping(MethodInfo methodInfo, Func<FunctionCallInfo, SqlExpression> callBackFunc)
        {
            CheckHelper.NotNull(methodInfo, nameof(methodInfo));
            this.SqlFunctionMappings[methodInfo] = callBackFunc;
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
        ///Guid To String;guid类型转string类型
        /// </summary>
        public bool GuidToString { get; set; } = false;
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
            this.CsharpTypeToDatabaseTypeNameMaps = new Dictionary<Type, string>();
            this.DatabaseTypeNameToCsharpTypeNameMaps = new Dictionary<string, string>();
            this.Id = Guid.NewGuid();
            TypeHandlers.TryAdd(Id, new Dictionary<Type, Type>());
            InitDefaultFunctionCall();
        }

        private SqlExpression StringBinaryMethodHandler(FunctionCallInfo x, SqlBinaryOperator sqlBinaryOperator, string likeLeft = "", string likeRight = "")
        {
            var p = x.FunctionParameters.First();

            if (sqlBinaryOperator.Equals(SqlBinaryOperator.Like))
            {
                AddLikeWildcards(p, x.DynamicParameters, likeLeft, likeRight);
            }

            return new SqlBinaryExpression()
            {
                Left = x.Body,
                Operator = sqlBinaryOperator,
                Right = p
            };
        }

        private SqlExpression StringOtherMethodHandler(FunctionCallInfo x, string databaseFunctionName, Func<string, string> methodCallBack)
        {
            if (x.Body is SqlStringExpression str)
            {
                str.Value = methodCallBack(str.Value);
                return str;
            }
            return GetSqlFunctionCallExpression(databaseFunctionName, new List<SqlExpression>() { x.Body });
        }

        private void InitDefaultFunctionCall()
        {
            var stringLikeMethodMappings = new List<KeyValuePair<MethodInfo, KeyValuePair<string, string>>>()
            {
                new KeyValuePair<MethodInfo, KeyValuePair<string, string>>(DefaultFunctionCall.Contains,
                    new KeyValuePair<string, string>("%", "%")),
                new KeyValuePair<MethodInfo, KeyValuePair<string, string>>(DefaultFunctionCall.StartsWith,
                    new KeyValuePair<string, string>("", "%")),
                new KeyValuePair<MethodInfo, KeyValuePair<string, string>>(DefaultFunctionCall.EndsWith,
                    new KeyValuePair<string, string>("%", "")),
            };
            foreach (var stringLikeMethodDic in stringLikeMethodMappings)
            {
                this.AddSqlFunctionMapping(stringLikeMethodDic.Key, x =>
                {
                    return StringBinaryMethodHandler(x, SqlBinaryOperator.Like, stringLikeMethodDic.Value.Key, stringLikeMethodDic.Value.Value);
                });
            }

            var stringOtherMethodMappings = new List<KeyValuePair<MethodInfo, KeyValuePair<string, Func<string, string>>>>()
            {
                new KeyValuePair<MethodInfo, KeyValuePair<string, Func<string, string>>>(DefaultFunctionCall.Trim,new KeyValuePair<string, Func<string, string>>("trim",p => p.Trim())),
                new KeyValuePair<MethodInfo, KeyValuePair<string, Func<string, string>>>(DefaultFunctionCall.TrimStart,new KeyValuePair<string, Func<string, string>>("ltrim",p => p.TrimStart())),
                new KeyValuePair<MethodInfo, KeyValuePair<string, Func<string, string>>>(DefaultFunctionCall.TrimEnd,new KeyValuePair<string, Func<string, string>>("rtrim",p => p.TrimEnd())),
                new KeyValuePair<MethodInfo, KeyValuePair<string, Func<string, string>>>(DefaultFunctionCall.ToLower,new KeyValuePair<string, Func<string, string>>("lower",p => p.ToLower())),
                new KeyValuePair<MethodInfo, KeyValuePair<string, Func<string, string>>>(DefaultFunctionCall.ToLowerInvariant,new KeyValuePair<string, Func<string, string>>("lower",p => p.ToLowerInvariant())),
                new KeyValuePair<MethodInfo, KeyValuePair<string, Func<string, string>>>(DefaultFunctionCall.ToUpper,new KeyValuePair<string, Func<string, string>>("upper",p => p.ToUpper())),
                new KeyValuePair<MethodInfo, KeyValuePair<string, Func<string, string>>>(DefaultFunctionCall.ToUpperInvariant,new KeyValuePair<string, Func<string, string>>("upper",p => p.ToUpperInvariant())),
            };

            foreach (var allMethodDic in stringOtherMethodMappings)
            {
                this.AddSqlFunctionMapping(allMethodDic.Key, x =>
                {
                    return StringOtherMethodHandler(x, allMethodDic.Value.Key, allMethodDic.Value.Value);
                });
            }

            this.AddSqlFunctionMapping(DefaultFunctionCall.Equals, x =>
            {
                return StringBinaryMethodHandler(x, SqlBinaryOperator.EqualTo);
            });
        }

        private SqlFunctionCallExpression GetSqlFunctionCallExpression(string functionName, List<SqlExpression> arguments)
        {
            return new SqlFunctionCallExpression()
            {
                Name = new SqlIdentifierExpression()
                {
                    Value = functionName
                },
                Arguments = arguments
            };
        }

        /// <summary>
        /// Add a wildcard like
        /// 添加like的通配符
        /// </summary>
        /// <returns></returns>
        private void AddLikeWildcards(SqlExpression sqlExpression, DynamicParameters parameters, string likeLeft = "", string likeRight = "")
        {
            if (sqlExpression is SqlVariableExpression sqlVariableExpression && parameters.GetParamInfos[sqlVariableExpression.Name].Value is string str && (likeLeft.HasText() || likeRight.HasText()))
            {
                parameters.GetParamInfos[sqlVariableExpression.Name].Value = likeLeft + str + likeRight;
            }
        }

        /// <summary>
        /// Unit of Work Type;工作单元类型
        /// </summary>
        public Type IUnitOfWorkType { get; }

        /// <summary>
        /// database Specific Provider Type;数据库特殊操作提供器类型
        /// </summary>
        public Type DatabaseSpecificProviderType { get; private set; }
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
        /// 绑定数据库特殊操作提供器类型
        /// </summary>
        public void BindDatabaseSpecificProviderType<T>() where T : IDatabaseSpecificProvider
        {
            this.DatabaseSpecificProviderType = typeof(T);
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
        /// Setting the mapping from C# types to database type names 设置c#类型到数据库类型名称的映射
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dbTypeName"></param>
        public void SetCsharpTypeToDatabaseTypeNameMap(Type type, string dbTypeName)
        {
            var nullableUnderlyingType = Nullable.GetUnderlyingType(type);
            var realType = nullableUnderlyingType ?? type;
            var nullableType = typeof(Nullable<>).MakeGenericType(realType);
            if (this.CsharpTypeToDatabaseTypeNameMaps.ContainsKey(realType))
            {
                this.CsharpTypeToDatabaseTypeNameMaps[realType] = dbTypeName;
                this.CsharpTypeToDatabaseTypeNameMaps[nullableType] = dbTypeName;
            }
            else
            {
                this.CsharpTypeToDatabaseTypeNameMaps.Add(realType, dbTypeName);
                this.CsharpTypeToDatabaseTypeNameMaps.Add(nullableType, dbTypeName);
            }
        }

        /// <summary>
        /// Setting the mapping from database type name to C# type name; 设置数据库类型名称到c#类型名称的映射
        /// </summary>
        /// <param name="dbTypeName"></param>
        /// <param name="csharpTypeName"></param>
        public void SetDatabaseTypeNameToCsharpTypeNameMap( string dbTypeName, string csharpTypeName)
        {
            if (this.DatabaseTypeNameToCsharpTypeNameMaps.ContainsKey(dbTypeName))
            {
                this.DatabaseTypeNameToCsharpTypeNameMaps[dbTypeName] = csharpTypeName;
              
            }
            else
            {
                this.DatabaseTypeNameToCsharpTypeNameMaps.Add(dbTypeName, csharpTypeName);
            }
        }

        /// <summary>
        /// Setting parameter type mapping;设置参数类型映射
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dbType"></param>
        public void SetParameterTypeMap(Type type, DbType dbType)
        {
            var nullableUnderlyingType = Nullable.GetUnderlyingType(type);
            var realType = nullableUnderlyingType ?? type;
            var nullableType = typeof(Nullable<>).MakeGenericType(realType);
            if (this.ParameterTypeMaps.ContainsKey(realType))
            {
                this.ParameterTypeMaps[realType] = dbType;
                this.ParameterTypeMaps[nullableType] = dbType;
            }
            else
            {
                this.ParameterTypeMaps.Add(realType, dbType);
                this.ParameterTypeMaps.Add(nullableType, dbType);
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
            var nullableUnderlyingType = Nullable.GetUnderlyingType(type);
            var realType = nullableUnderlyingType ?? type;
            var nullableType = typeof(Nullable<>).MakeGenericType(realType);
            this.InternalSetTypeHandler(type, t);
            this.InternalSetTypeHandler(nullableType, t);
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
