using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Castle.DynamicProxy.Internal;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SqlOnline.Utils;
using SummerBoot.Core;

namespace SummerBoot.Repository
{
    public class RepositoryAspectSupport
    {
        private IPageable pageable;
        private IServiceProvider ServiceProvider { set; get; }

        private IDbConnection dbConnection;

        private IDbTransaction dbTransaction;

        private IUnitOfWork uow;
        private IDbFactory dbFactory;

        private void Init()
        {
            //先获得工作单元和数据库工厂以及序列化器
            uow = ServiceProvider.GetService<IUnitOfWork>();
            dbFactory = ServiceProvider.GetService<IDbFactory>();
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

                OpenDb();
                var sql = selectAttribute.Sql;

                var result = new Page<T>() { };
                //oracle这坑逼数据库需要单独处理
                if (repositoryOption.IsOracle)
                {
                    var countSql = $"select count(*) from ({sql}) ";
                    var pageSql = pageable != null ?
                        $"select * from (SELECT TMP_PAGE.*, ROWNUM ROW_ID FROM ({sql}) TMP_PAGE)    WHERE ROW_ID <{pageable.PageSize * (pageable.PageNumber) + 1}  AND ROW_ID >={pageable.PageSize * (pageable.PageNumber - 1) + 1}" :
                        $"select * from ({sql})";
                    var count = dbConnection.QueryFirst<int>(countSql, dbArgs);
                    var resultList = dbConnection.Query<T>(pageSql, dbArgs).ToList();
                    result.TotalPages = count;
                    result.Data = resultList;
                }
                else
                {
                    sql = GetPageSql(repositoryOption, sql);
                    var dbResult = dbConnection.QueryMultiple(sql, dbArgs);
                    var count = dbResult.Read<int>().First();
                    var resultList = dbResult.Read<T>().ToList();

                    result.TotalPages = count;
                    result.Data = resultList;
                }

                if (pageable != null)
                {
                    result.PageSize = pageable.PageSize;
                    result.PageNumber = pageable.PageNumber;
                }

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
                var sql = selectAttribute.Sql;

                OpenDb();
                var result = new Page<T>() { };
                //oracle这坑逼数据库需要单独处理
                if (repositoryOption.IsOracle)
                {
                    var countSql = $"select count(*) from ({sql}) ";
                    var pageSql = pageable != null ?
                        $"select * from (SELECT TMP_PAGE.*, ROWNUM ROW_ID FROM ({sql}) TMP_PAGE)    WHERE ROW_ID <{pageable.PageSize * (pageable.PageNumber) + 1}  AND ROW_ID >={pageable.PageSize * (pageable.PageNumber - 1) + 1}" :
                        $"select * from ({sql})";
                    var count = await dbConnection.QueryFirstAsync<int>(countSql, dbArgs);
                    var resultList = await dbConnection.QueryAsync<T>(pageSql, dbArgs);
                    result.TotalPages = count;
                    result.Data = resultList.ToList();
                }
                else
                {
                    sql = GetPageSql(repositoryOption, sql);
                    var dbResult = await dbConnection.QueryMultipleAsync(sql, dbArgs);
                    var count = dbResult.Read<int>().First();
                    var resultList = dbResult.Read<T>().ToList();

                    result.TotalPages = count;
                    result.Data = resultList;
                }

                if (pageable != null)
                {
                    result.PageSize = pageable.PageSize;
                    result.PageNumber = pageable.PageNumber;
                }

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
                OpenDb();
                if (baseTypeIsSameReturnType)
                {
                    var queryResult = dbConnection.Query<T>(sql, dbArgs);
                    return queryResult.FirstOrDefault();
                }
                else
                {
                    var queryResult = dbConnection.Query<TBaseType>(sql, dbArgs);
                    if (targetType.IsCollection())
                    {
                        return (T)queryResult;
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
                var sql = selectAttribute.Sql;
                var repositoryOption = serviceProvider.GetService<RepositoryOption>();
                //获得动态参数
                var dbArgs = GetParameters(method, args);

                OpenDb();

                if (baseTypeIsSameReturnType)
                {
                    var queryResult = await dbConnection.QueryAsync<T>(sql, dbArgs);
                    return queryResult.FirstOrDefault();
                }
                else
                {
                    var queryResult = await dbConnection.QueryAsync<TBaseType>(sql, dbArgs);
                    if (targetType.IsCollection())
                    {
                        return (T)queryResult;
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
            OpenDb();
            var executeResult = dbConnection.Execute(sql, dbArgs, dbTransaction);
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

            OpenDb();
            var executeResult = await dbConnection.ExecuteAsync(sql, dbArgs, dbTransaction);
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
            var dbArgs = new DynamicParameters();
            var parameterInfos = method.GetParameters();
            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var parameterType = parameterInfos[i].ParameterType;
                var parameterTypeIsString = parameterType.IsString();
                //如果是分页参数直接跳过
                if (typeof(IPageable).IsAssignableFrom(parameterType))
                {
                    pageable = (IPageable)args[i];
                    continue;
                }
                //如果是值类型或者字符串直接添加到参数里
                if (parameterType.IsValueType || parameterTypeIsString)
                {
                    dbArgs.Add(parameterInfos[i].Name, args[i]);
                }
                //如果是类，则读取属性值，然后添加到参数里
                else if (parameterType.IsClass)
                {
                    var properties = parameterType.GetTypeInfo().DeclaredProperties;
                    foreach (PropertyInfo info in properties)
                    {
                        var propertyType = info.PropertyType;
                        var propertyTypeIsString = propertyType.GetTypeInfo() == typeof(string);
                        if (propertyType.IsValueType || propertyTypeIsString)
                        {
                            dbArgs.Add(info.Name, info.GetValue(args[i]));
                        }
                    }
                }
            }

            return dbArgs;
        }

        /// <summary>
        /// 获得分页数据
        /// </summary>
        /// <param name="repositoryOption"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        private string GetPageSql(RepositoryOption repositoryOption, string sql)
        {
            var resultSql = $"select count(*) from ({sql});";
            var dbName = repositoryOption.DbConnectionType.FullName;
            if (repositoryOption.IsSqlite)
            {
                resultSql += pageable != null ?
                    $"select * from ({sql}) limit {pageable.PageSize} offset {pageable.PageSize * (pageable.PageNumber - 1)}" :
                    $"select * from ({sql})";
            }
            else if (repositoryOption.IsMysql)
            {
                resultSql = $"select count(*) from ({sql}) tmp1;";
                resultSql += pageable != null ?
                    $"select * from ({sql}) tmp2 limit {pageable.PageSize * (pageable.PageNumber - 1)},{pageable.PageSize}" :
                    $"select * from ({sql}) tmp2";
            }
            else if (repositoryOption.IsSqlServer)
            {
                resultSql = $"select count(*) from ({sql}) tmp;select * from ({sql}) tmp2";
            }
            else
            {
                throw new Exception("not support");
            }

            return resultSql;
        }
    }
}