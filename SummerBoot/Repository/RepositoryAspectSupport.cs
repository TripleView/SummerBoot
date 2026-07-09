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
using SqlParser.Net;
using SqlParser.Net.Ast.Expression;
using SummerBoot.Repository.ExpressionParser;

namespace SummerBoot.Repository
{
    public class RepositoryAspectSupport
    {
        public RepositoryAspectSupport(IUnitOfWork uow)
        {
            this.uow = uow;
            this.dbFactory = uow.DbFactory;
            this.databaseUnit = dbFactory.DatabaseUnit;
        }

        private IServiceProvider ServiceProvider { set; get; }

        private IDbConnection dbConnection;

        private IDbTransaction dbTransaction;

        private IUnitOfWork uow;

        private IDbFactory dbFactory;

        private DatabaseUnit databaseUnit;

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
            var configuration = ServiceProvider.GetService<IConfiguration>();
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
            Pageable pageable = args.FirstOrDefault(x => x is Pageable) as Pageable;
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

                var result = new Page<T>() { };
                var sqlSelectExpression = DbUtils.Parse(sql, databaseUnit.DbType);

                var toPageOutputDto = new NewDbExpressionVisitor(databaseUnit).SqlSelectExpressionToPage(sqlSelectExpression, pageable);

                dbArgs.Add(toPageOutputDto.DynamicParameters);
                var count = dbConnection.QueryFirstOrDefault<int>(databaseUnit, toPageOutputDto.CountSqlSelectExpression.ToSql(databaseUnit.DbType), dbArgs, transaction: dbTransaction);
                var resultList = (dbConnection.Query<T>(databaseUnit, toPageOutputDto.PageSqlSelectExpression.ToSql(databaseUnit.DbType), dbArgs, transaction: dbTransaction)).ToList();
                result.TotalPages = count;
                result.Data = resultList;

                result.PageSize = pageable.PageSize;
                result.PageNumber = pageable.PageNumber;

                CloseDb();

                return result;
            }

            throw new Exception("can not process method name:" + method.Name);
        }

        public async Task<Page<T>> PageBaseExecuteAsync<T>(MethodInfo method, object[] args, IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Pageable pageable = args.FirstOrDefault(x => x is Pageable) as Pageable;
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

                var result = new Page<T>() { };
                var sqlSelectExpression = DbUtils.Parse(sql, databaseUnit.DbType);

                var toPageOutputDto = new NewDbExpressionVisitor(databaseUnit).SqlSelectExpressionToPage(sqlSelectExpression, pageable);

                dbArgs.Add(toPageOutputDto.DynamicParameters);
                var count = await dbConnection.QueryFirstOrDefaultAsync<int>(databaseUnit, toPageOutputDto.CountSqlSelectExpression.ToSql(databaseUnit.DbType), dbArgs, transaction: dbTransaction);
                var resultList = (await dbConnection.QueryAsync<T>(databaseUnit, toPageOutputDto.PageSqlSelectExpression.ToSql(databaseUnit.DbType), dbArgs, transaction: dbTransaction)).ToList();
                result.TotalPages = count;
                result.Data = resultList;

                result.PageSize = pageable.PageSize;
                result.PageNumber = pageable.PageNumber;

                CloseDb();

                return result;
            }

            throw new Exception("can not process method name:" + method.Name);
        }


        public T BaseExecute<T, TBaseType>(MethodInfo method, object[] args, IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Pageable pageable = null;

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

            Pageable pageable = null;

            //处理select逻辑
            var selectAttribute = method.GetCustomAttribute<SelectAttribute>();
            if (selectAttribute != null)
            {


                //获得动态参数
                var dbArgs = GetParameters(method, args);
                var sql = selectAttribute.Sql;
                sql = GetValueByConfiguration(sql);

                OpenDb();

                if (baseTypeIsSameReturnType)
                {
                    var queryResult = await dbConnection.QueryAsync<T>(databaseUnit, sql, dbArgs, transaction: dbTransaction);
                    return queryResult.FirstOrDefault();
                }
                else
                {
                    var queryResult = (await dbConnection.QueryAsync<TBaseType>(databaseUnit, sql, dbArgs, transaction: dbTransaction)).ToList();
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

            Pageable pageable = null;

            //获得动态参数
            var dbArgs = GetParameters(method, args);
            var deleteAttribute = method.GetCustomAttribute<DeleteAttribute>();
            var updateAttribute = method.GetCustomAttribute<UpdateAttribute>();
            if (deleteAttribute == null && updateAttribute == null) return 0;
            var sql = updateAttribute != null ? updateAttribute.Sql : deleteAttribute.Sql;
            sql = GetValueByConfiguration(sql);

            OpenDb();
            var executeResult = dbConnection.Execute(databaseUnit, sql, dbArgs, transaction: dbTransaction);
            CloseDb();

            return executeResult;
        }

        public async Task<int> BaseExecuteReturnCountAsync(MethodInfo method, object[] args,
            IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            //获得动态参数
            var dbArgs = GetParameters(method, args);
            var deleteAttribute = method.GetCustomAttribute<DeleteAttribute>();
            var updateAttribute = method.GetCustomAttribute<UpdateAttribute>();
            if (deleteAttribute == null && updateAttribute == null) return 0;
            var sql = updateAttribute != null ? updateAttribute.Sql : deleteAttribute.Sql;
            sql = GetValueByConfiguration(sql);
            OpenDb();
            var executeResult = await dbConnection.ExecuteAsync(databaseUnit, sql, dbArgs, transaction: dbTransaction);
            CloseDb();

            return executeResult;
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
            IPageable pageable = null;
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
                    continue;
                }

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
                        var propertyTypeIsString = propertyType.IsString();
                        if (propertyType.IsValueType || propertyTypeIsString || propertyType.IsCollection())
                        {
                            dbArgs.Add(info.Name, info.GetValue(args[i]), valueType: propertyType);
                        }
                    }
                }
            }

            return dbArgs;
        }
    }
}