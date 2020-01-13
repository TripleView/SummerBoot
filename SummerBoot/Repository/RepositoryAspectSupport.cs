using System;
using System.Linq;
using System.Reflection;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using SqlOnline.Utils;
using SummerBoot.Core;

namespace SummerBoot.Repository
{
    public class RepositoryAspectSupport<T>
    {
        //IRepository接口里的固定方法名
        private string[] solidMethodNames = new string[] { "GetAll", "Get", "Insert", "Update", "Delete", "GetAllAsync", "GetAsync", "InsertAsync", "UpdateAsync", "DeleteAsync" };

        private IServiceProvider ServiceProvider { set; get; }

        public object Execute(Func<object> invoker, MethodInfo method, object[] args, IServiceProvider serviceProvider)
        {
            //如果是IRepository接口里的固定方法，则调用BaseRepository获取结果
            if (method.DeclaringType?.Name == "IRepository`1" && solidMethodNames.Contains(method.Name) && typeof(T).IsClass)
            {
                var result = ProcessFixedMethodCall(method, args);
                return result;
            }

            //如果是自定义方法，则使用dapper获得结果

            //先获得工作单元和数据库工厂
            var uow = serviceProvider.GetService<IUnitOfWork>();
            var db = serviceProvider.GetService<IDbFactory>();

            //判断方法返回值是否为异步类型
            var isAsyncReturnType = method.ReturnType.IsAsyncType();
            //返回类型
            var returnTypeTmp = isAsyncReturnType ? method.ReturnType.GenericTypeArguments.First() : method.ReturnType;

            var isGenericType = returnTypeTmp.IsGenericType;
            var isCollection = returnTypeTmp.IsCollection();
            var isEnumerable = returnTypeTmp.IsEnumerable();
            var isQueryable = returnTypeTmp.IsQueryable();
            var isValueType = returnTypeTmp.IsValueType;
            var isString = returnTypeTmp.IsString();

            //最底层的返回类型
            Type returnType = isGenericType ? returnTypeTmp.GetGenericArguments().FirstOrDefault() : returnTypeTmp;

            if (returnType != typeof(T))
            {
                return invoker();
            }

            //获得动态参数
            var dbArgs = GetParameters(method, args);

            //处理select逻辑
            var selectAttribute = method.GetCustomAttribute<SelectAttribute>();
            if (selectAttribute != null)
            {
                return ProcessSelectAttribute(selectAttribute, db, dbArgs, isCollection, isQueryable, isEnumerable, isString);
            }

            //处理update逻辑
            var updateAttribute = method.GetCustomAttribute<UpdateAttribute>();
            if (updateAttribute != null)
            {
                return ProcessUpdateAttribute(updateAttribute, db, dbArgs, uow);
            }

            //处理delete逻辑
            var deleteAttribute = method.GetCustomAttribute<DeleteAttribute>();
            if (updateAttribute != null)
            {
                return ProcessDeleteAttribute(deleteAttribute, db, dbArgs, uow);
            }


            throw new Exception("can not process method name:" + method.Name);
        }

        /// <summary>
        /// 处理固定方法调用
        /// </summary>
        /// <returns></returns>
        private object ProcessFixedMethodCall(MethodInfo method, object[] args)
        {
            var baseRepositoryType = typeof(BaseRepository<>).MakeGenericType(typeof(T));

            var callMethod = baseRepositoryType.GetMethods()
                .FirstOrDefault(it => it.Name == method.Name && it.ReturnType == method.ReturnType && it.GetGenericArguments() == method.GetGenericArguments());

            var baseRepository = ServiceProvider.GetService(baseRepositoryType);

            var result = callMethod?.Invoke(baseRepository, args);

            return result;
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

        private object ProcessSelectAttribute(SelectAttribute attribute, IDbFactory db, DynamicParameters args, bool isCollection, bool isQueryable, bool isEnumerable, bool isString)
        {
            var sql = attribute.Sql;

            var queryResult = db.ShortDbConnection.Query<T>(sql, args);

            var result = new object();

            if (isCollection)
            {
                result = queryResult;
            }
            else if (isQueryable)
            {
                result = queryResult.AsQueryable();
            }
            else if (isEnumerable && !isString)
            {
                result = queryResult.AsEnumerable();
            }
            else
            {
                result = queryResult.FirstOrDefault();
            }

            return result;
        }

        private object ProcessUpdateAttribute(UpdateAttribute attribute, IDbFactory db, DynamicParameters args, IUnitOfWork uow)
        {
            var sql = attribute.Sql;
            var updateResult = 0;
            if (uow == null)
            {
                updateResult = db.ShortDbConnection.Execute(sql, args);
                return updateResult;
            }

            var dbcon = uow.ActiveNumber == 0 ? db.ShortDbConnection : db.LongDbConnection;
            updateResult = dbcon.Execute(sql, args, db.LongDbTransaction);

            if (uow.ActiveNumber == 0)
            {
                dbcon.Close();
                dbcon.Dispose();
            }

            return updateResult;
        }
        
        private object ProcessDeleteAttribute(DeleteAttribute attribute, IDbFactory db, DynamicParameters args, IUnitOfWork uow)
        {
            var sql = attribute.Sql;
            var deleteResult = 0;
            if (uow == null)
            {
                deleteResult = db.ShortDbConnection.Execute(sql, args);
                return deleteResult;
            }

            var dbcon = uow.ActiveNumber == 0 ? db.ShortDbConnection : db.LongDbConnection;
            deleteResult = dbcon.Execute(sql, args, db.LongDbTransaction);

            if (uow.ActiveNumber == 0)
            {
                dbcon.Close();
                dbcon.Dispose();
            }

            return deleteResult;
        }
    }
}