using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Core;
using SummerBoot.Repository.Attributes;
using SummerBoot.Repository.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SummerBoot.Repository
{
    public class RepositoryAspectSupport
    {
        public RepositoryAspectSupport(IUnitOfWork uow)
        {
            this.uow = uow;
            this.dbFactory = uow.DbFactory;
        }
        private IPageable pageable;
        private IServiceProvider ServiceProvider { set; get; }

        private IDbConnection dbConnection;

        private IDbTransaction dbTransaction;

        private IUnitOfWork uow;

        private IDbFactory dbFactory;

        private DatabaseUnit databaseUnit;

        public void SetDatabaseUnit(DatabaseUnit databaseUnit)
        {
            this.databaseUnit = databaseUnit;
        }

        /// <summary>
        /// 参数字典
        /// </summary>
        private Dictionary<string, object> parameterDictionary = new Dictionary<string, object>();
        /// <summary>
        /// 仓储参数
        /// </summary>
        private RepositoryOption repositoryOption;

        private IConfiguration configuration;

        private void Init()
        {
            //先获得工作单元和数据库工厂以及序列化器
            //uow = ServiceProvider.GetService<IUnitOfWork>();
            //dbFactory = ServiceProvider.GetService<IDbFactory>();
            repositoryOption = ServiceProvider.GetService<RepositoryOption>();
            configuration = ServiceProvider.GetService<IConfiguration>();
            parameterDictionary.Clear();
            pageable = null;
        }

        protected void OpenDb()
        {
            dbConnection = uow.ActiveNumber == 0 ? dbFactory.GetDbConnection() : dbFactory.GetDbTransaction().Connection;
            dbTransaction = uow.ActiveNumber == 0 ? null : dbFactory.GetDbTransaction();
        }

        protected void CloseDb()
        {
            if (uow.ActiveNumber == 0)
            {
                dbConnection.Close();
                dbConnection.Dispose();
            }
        }

        /// <summary>
        /// 通过配置文件获取具体的值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetValueByConfiguration(string value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                return value;
            }
            var str = Regex.Replace(value, "\\$\\{[^\\}]*\\}", match =>
            {
                var matchValue = match.Value;
                if (matchValue.Length >= 3)
                {
                    if (matchValue.Substring(0, 2) == "${")
                    {
                        matchValue = matchValue.Substring(2);
                    }
                    if (matchValue.Substring(matchValue.Length - 1, 1) == "}")
                    {
                        matchValue = matchValue.Substring(0, matchValue.Length - 1);
                    }

                    matchValue = matchValue.Trim();
                    matchValue = configuration.GetSection(matchValue).Value;
                    return matchValue;
                }
                return "";

            }, RegexOptions.Compiled);
            return str;
        }


        public Page<T> PageBaseExecute<T>(MethodInfo method, object[] args, IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Init();
            //处理select逻辑
            var selectAttribute = method.GetCustomAttribute<SelectAttribute>();
            if (selectAttribute != null)
            {
                var repositoryOption = serviceProvider.GetService<RepositoryOption>();
                //获得动态参数
                var dbArgs = GetParameters(method, args);
                if (pageable == null)
                {
                    throw new Exception("method argument must have pageable");
                }

                OpenDb();
                var sql = selectAttribute.Sql;
                sql = GetValueByConfiguration(sql);
                if (!sql.Contains("order by", StringComparison.OrdinalIgnoreCase))
                {
                    throw new NotSupportedException("sql must contain order by clause");
                }
                sql = ReplaceSqlBindWhereCondition(sql);
                var result = new Page<T>() { };

                //SqlParser.SqlParser parser;

                //if (databaseUnit.IsOracle)
                //{
                //    parser = new OracleParser();
                //}
                //else if (databaseUnit.IsSqlServer)
                //{
                //    parser = new SqlServerParser();
                //}
                //else if (databaseUnit.IsMysql)
                //{
                //    parser = new MysqlParser();
                //}
                //else if (databaseUnit.IsPgsql)
                //{
                //    parser = new PgsqlParser();
                //}
                //else
                //{
                //    parser = new SqliteParser();
                //}

                //var parseResult = parser.ParserPage(sql, pageable.PageNumber, pageable.PageSize);

                //ChangeDynamicParameters(parseResult.SqlParameters, dbArgs);

                //var count = dbConnection.QueryFirstOrDefault<int>(databaseUnit,parseResult.CountSql, dbArgs, transaction: dbTransaction);
                //var resultList = dbConnection.Query<T>(databaseUnit, parseResult.PageSql, dbArgs, transaction: dbTransaction).ToList();
                //result.TotalPages = count;
                //result.Data = resultList;

                //result.PageSize = pageable.PageSize;
                //result.PageNumber = pageable.PageNumber;

                CloseDb();

                return result;
            }

            throw new Exception("can not process method name:" + method.Name);
        }

        public async Task<Page<T>> PageBaseExecuteAsync<T>(MethodInfo method, object[] args, IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Init();
            //处理select逻辑
            var selectAttribute = method.GetCustomAttribute<SelectAttribute>();
            if (selectAttribute != null)
            {
                var repositoryOption = serviceProvider.GetService<RepositoryOption>();
                //获得动态参数
                var dbArgs = GetParameters(method, args);
                if (pageable == null)
                {
                    throw new Exception("method argument must have pageable");
                }

                OpenDb();
                var sql = selectAttribute.Sql;
                sql = GetValueByConfiguration(sql);
                if (!sql.Contains("order by", StringComparison.OrdinalIgnoreCase))
                {
                    throw new NotSupportedException("sql must contain order by clause");
                }
                sql = ReplaceSqlBindWhereCondition(sql);

                var result = new Page<T>() { };

                //SqlParser.SqlParser parser;

                //if (databaseUnit.IsOracle)
                //{
                //    parser = new OracleParser();
                //}
                //else if (databaseUnit.IsSqlServer)
                //{
                //    parser = new SqlServerParser();
                //}
                //else if (databaseUnit.IsMysql)
                //{
                //    parser = new MysqlParser();
                //}
                //else if (databaseUnit.IsPgsql)
                //{
                //    parser = new PgsqlParser();
                //}
                //else
                //{
                //    parser = new SqliteParser();
                //}

                //var parseResult = parser.ParserPage(sql, pageable.PageNumber, pageable.PageSize);

                //ChangeDynamicParameters(parseResult.SqlParameters, dbArgs);

                //var count = await dbConnection.QueryFirstOrDefaultAsync<int>(databaseUnit, parseResult.CountSql, dbArgs, transaction: dbTransaction);
                //var resultList = (await dbConnection.QueryAsync<T>(databaseUnit, parseResult.PageSql, dbArgs, transaction: dbTransaction)).ToList();
                //result.TotalPages = count;
                //result.Data = resultList;

                //result.PageSize = pageable.PageSize;
                //result.PageNumber = pageable.PageNumber;

                CloseDb();

                return result;
            }

            throw new Exception("can not process method name:" + method.Name);
        }

        public T BaseExecute<T, TBaseType>(MethodInfo method, object[] args, IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Init();

            var targetType = typeof(T);
            var baseTypeIsSameReturnType = typeof(T) == typeof(TBaseType);

            //获得动态参数
            var dbArgs = GetParameters(method, args);

            //处理select逻辑
            var selectAttribute = method.GetCustomAttribute<SelectAttribute>();
            if (selectAttribute != null)
            {
                var sql = selectAttribute.Sql;
                sql = GetValueByConfiguration(sql);
                sql = ReplaceSqlBindWhereCondition(sql);

                OpenDb();
                if (baseTypeIsSameReturnType)
                {
                    var queryResult = dbConnection.QueryFirstOrDefault<T>(databaseUnit, sql, dbArgs, transaction: dbTransaction);
                    return queryResult;
                }
                else
                {
                    var queryResult = dbConnection.Query<TBaseType>(databaseUnit, sql, dbArgs, transaction: dbTransaction).ToList();
                    if (targetType.IsCollection())
                    {
                        return (T)(object)queryResult;
                    }
                }
                CloseDb();
            }

            throw new Exception("can not process method name:" + method.Name);
        }

        public async Task<T> BaseExecuteAsync<T, TBaseType>(MethodInfo method, object[] args, IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            var targetType = typeof(T);
            var baseTypeIsSameReturnType = typeof(T) == typeof(TBaseType);

            Init();

            //处理select逻辑
            var selectAttribute = method.GetCustomAttribute<SelectAttribute>();
            if (selectAttribute != null)
            {


                //获得动态参数
                var dbArgs = GetParameters(method, args);
                var sql = selectAttribute.Sql;
                sql = GetValueByConfiguration(sql);
                sql = ReplaceSqlBindWhereCondition(sql);

                OpenDb();

                if (baseTypeIsSameReturnType)
                {
                    var queryResult = await dbConnection.QueryAsync<T>(databaseUnit, sql, dbArgs, transaction: dbTransaction);
                    return queryResult.FirstOrDefault();
                }
                else
                {
                    var queryResult =( await dbConnection.QueryAsync<TBaseType>(databaseUnit, sql, dbArgs, transaction: dbTransaction)).ToList();
                    if (targetType.IsCollection())
                    {
                        return (T)(object)queryResult;
                    }
                }
            }

            throw new Exception("can not process method name:" + method.Name);
        }

        public void BaseExecuteNoReturn(MethodInfo method, object[] args, IServiceProvider serviceProvider)
        {
            BaseExecuteReturnCount(method, args, serviceProvider);
        }

        public async Task BaseExecuteNoReturnAsync(MethodInfo method, object[] args,
            IServiceProvider serviceProvider)
        {
            await BaseExecuteReturnCountAsync(method, args, serviceProvider);
        }

        public int BaseExecuteReturnCount(MethodInfo method, object[] args, IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;

            Init();

            //获得动态参数
            var dbArgs = GetParameters(method, args);
            var deleteAttribute = method.GetCustomAttribute<DeleteAttribute>();
            var updateAttribute = method.GetCustomAttribute<UpdateAttribute>();
            if (deleteAttribute == null && updateAttribute == null) return 0;
            var sql = updateAttribute != null ? updateAttribute.Sql : deleteAttribute.Sql;
            sql = GetValueByConfiguration(sql);
            sql = ReplaceSqlBindWhereCondition(sql);

            OpenDb();
            var executeResult = dbConnection.Execute(databaseUnit, sql, dbArgs, transaction: dbTransaction);
            CloseDb();

            return executeResult;
        }

        public async Task<int> BaseExecuteReturnCountAsync(MethodInfo method, object[] args,
            IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Init();
            //获得动态参数
            var dbArgs = GetParameters(method, args);
            var deleteAttribute = method.GetCustomAttribute<DeleteAttribute>();
            var updateAttribute = method.GetCustomAttribute<UpdateAttribute>();
            if (deleteAttribute == null && updateAttribute == null) return 0;
            var sql = updateAttribute != null ? updateAttribute.Sql : deleteAttribute.Sql;
            sql = GetValueByConfiguration(sql);
            sql = ReplaceSqlBindWhereCondition(sql);
            OpenDb();
            var executeResult = await dbConnection.ExecuteAsync(databaseUnit, sql, dbArgs, transaction: dbTransaction);
            CloseDb();

            return executeResult;
        }

        /// <summary>
        /// 替换sql语句里的bindWhere条件的语句，如{{ and a.name=@name}}
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private string ReplaceSqlBindWhereCondition(string sql)
        {
            var bindWhereParameterNames = parameterDictionary.Keys.ToList();

            //参数前缀
            var parameterPrefix = "";
            switch (databaseUnit.DatabaseType)
            {
                case DatabaseType.Mysql:
                case DatabaseType.SqlServer:
                case DatabaseType.Pgsql:
                    parameterPrefix = "@";
                    break;
                case DatabaseType.Sqlite:
                case DatabaseType.Oracle:
                    parameterPrefix = ":";
                    break;
            }

            var tempSql = Regex.Replace(sql, "\\{\\{[^\\}]*\\}\\}", match =>
            {
                string matchValue = match.Value;
                var hasFind = false;
                foreach (var name in bindWhereParameterNames)
                {
                    var parameterName = parameterPrefix + name;
                    var matchResult = Regex.Match(matchValue, $"{parameterPrefix}\\w*");
                    if (matchResult.Success && matchResult.Value.ToLower() == parameterName.ToLower())
                    {
                        hasFind = true;
                    }
                }
                //如果参数有值才返回该条件判断语句，否则返回空
                if (hasFind)
                {
                    return matchValue.Replace("{{", "").Replace("}}", "");
                }

                return "";

            }, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            return tempSql;
        }
        /// <summary>
        /// 获取实际参数值
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private DynamicParameters GetParameters(MethodInfo method, object[] args)
        {
            //获取参数
            var dbArgs = new DynamicParameters();
            var parameterInfos = method.GetParameters();
            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var parameterType = parameterInfos[i].ParameterType;
                var parameterTypeIsString = parameterType.IsString();
                //如果是分页参数直接跳过
                if (typeof(IPageable).IsAssignableFrom(parameterType))
                {
                    if (pageable != null)
                    {
                        throw new NotSupportedException("2 IPageable parameters are not supported");
                    }
                    pageable = (IPageable)args[i];
                }

                //查找所有条件语句替换
                var bindWhere = parameterType.IsGenericType && typeof(WhereItem<>).IsAssignableFrom(parameterType.GetGenericTypeDefinition());
                if (bindWhere)
                {
                    var paramAttribute = parameterInfos[i].GetCustomAttribute<ParamAttribute>();
                    var argValue = args[i];
                    var parameterName = paramAttribute != null && paramAttribute.Alias.HasText()
                        ? paramAttribute.Alias
                        : parameterInfos[i].Name;

                    if (parameterName.IsNullOrWhiteSpace())
                    {
                        throw new ArgumentNullException(nameof(argValue));
                    }

                    if (parameterName.IsNullOrWhiteSpace())
                    {
                        throw new ArgumentNullException(nameof(argValue));
                    }

                    var value = parameterType.GetProperty("Value").GetValue(argValue);
                    var active = (bool)parameterType.GetProperty("Active").GetValue(argValue);

                    if (value != null && active)
                    {
                        var valueType = parameterType.GetGenericArguments()[0];
                        parameterDictionary.Add(parameterName, value);
                        dbArgs.Add(parameterName, value,valueType: valueType);
                    }
                }
                else
                {
                    //如果是值类型或者字符串直接添加到参数里
                    if (parameterType.IsValueType || parameterTypeIsString || parameterType.IsCollection())
                    {
                        dbArgs.Add(parameterInfos[i].Name, args[i], valueType: parameterType);
                    }
                    //如果是类，则读取属性值，然后添加到参数里
                    else if (parameterType.IsClass)
                    {
                        var properties = parameterType.GetProperties();
                        foreach (PropertyInfo info in properties)
                        {
                            var propertyType = info.PropertyType;
                            var propertyTypeIsString = propertyType.GetTypeInfo() == typeof(string);
                            if (propertyType.IsValueType || propertyTypeIsString || propertyType.IsCollection())
                            {
                                dbArgs.Add(info.Name, info.GetValue(args[i]), valueType: propertyType);
                            }
                            //查找所有条件语句替换
                            var propertyBindWhere = propertyType.IsGenericType && typeof(WhereItem<>).IsAssignableFrom(propertyType.GetGenericTypeDefinition());
                            if (propertyBindWhere)
                            {
                                var paramAttribute = parameterInfos[i].GetCustomAttribute<ParamAttribute>();
                                var argValue = info.GetValue(args[i]);
                                var parameterName = paramAttribute != null && paramAttribute.Alias.HasText()
                                    ? paramAttribute.Alias
                                    : parameterInfos[i].Name;

                                if (parameterName.IsNullOrWhiteSpace())
                                {
                                    throw new ArgumentNullException(nameof(argValue));
                                }

                                if (parameterName.IsNullOrWhiteSpace())
                                {
                                    throw new ArgumentNullException(nameof(argValue));
                                }

                                var value = propertyType.GetProperty("Value").GetValue(argValue);
                                var active = (bool)propertyType.GetProperty("Active").GetValue(argValue);

                                if (value != null && active)
                                {
                                    parameterDictionary.Add(parameterName, value);
                                    dbArgs.Add(parameterName, value, valueType: propertyType.GetGenericArguments()[0]);
                                }
                            }
                        }
                    }
                }


            }

            return dbArgs;
        }

        //protected void ChangeDynamicParameters(List<SqlParameter> originSqlParameters, DynamicParameters dynamicParameters)
        //{
        //    if (originSqlParameters == null || originSqlParameters.Count == 0)
        //    {
        //        return;
        //    }

        //    foreach (var parameter in originSqlParameters)
        //    {
        //        dynamicParameters.Add(parameter.ParameterName, parameter.Value,valueType:parameter.ParameterType);
        //    }
        //}

    }
}