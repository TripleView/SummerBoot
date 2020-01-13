using System;
using System.Linq;
using System.Reflection;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using SqlOnline.Utils;
using SummerBoot.Core;

namespace SummerBoot.Repository
{
    public class RepositoryAspectSupport2
    {
        //IRepository接口里的固定方法名
        private string[] solidMethodNames = new string[] { "GetAll", "Get", "Insert", "Update", "Delete", "GetAllAsync", "GetAsync", "InsertAsync", "UpdateAsync", "DeleteAsync" };

        private IServiceProvider ServiceProvider { set; get; }

        public object Execute( MethodInfo method, object[] args, IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;

            //如果是IRepository接口里的固定方法，则调用BaseRepository获取结果
            var underlyingType = method.ReturnType.GetUnderlyingType();
            if (method.DeclaringType?.Name == "IRepository`1" && solidMethodNames.Contains(method.Name) && underlyingType.IsClass)
            {
                var result = ProcessFixedMethodCall(method, args, underlyingType);
                return result;
            }

            //如果是自定义方法，则使用dapper获得结果

            //先获得工作单元和数据库工厂以及序列化器
            var uow = serviceProvider.GetService<IUnitOfWork>();
            var db = serviceProvider.GetService<IDbFactory>();
            var serialization = serviceProvider.GetService<ISerialization>();

            //获得动态参数
            var dbArgs = GetParameters(method, args);

            //处理select逻辑
            var selectAttribute = method.GetCustomAttribute<SelectAttribute>();
            if (selectAttribute != null)
            {
                var sql = selectAttribute.Sql;

                var queryResult = db.ShortDbConnection.Query(underlyingType, sql, dbArgs);

                var tmpResult = method.ReturnType.IsCollection() ? queryResult : queryResult.FirstOrDefault();

                return serialization.DeserializeObject(serialization.SerializeObject(tmpResult), method.ReturnType);
            }

            //处理update逻辑
            var updateAttribute = method.GetCustomAttribute<UpdateAttribute>();
            if (updateAttribute != null)
            {
                return ProcessUpdateAttribute(updateAttribute, db, dbArgs,uow);
            }

            //处理delete逻辑
            var deleteAttribute = method.GetCustomAttribute<DeleteAttribute>();
            if (updateAttribute != null)
            {
                return ProcessDeleteAttribute(deleteAttribute, db, dbArgs, uow);
            }


            throw new Exception("can not process method name:"+method.Name);
        }

        /// <summary>
        /// 处理固定方法调用
        /// </summary>
        /// <returns></returns>
        private object ProcessFixedMethodCall(MethodInfo method, object[] args,Type underlyingType)
        {
            var baseRepositoryType = typeof(BaseRepository<>).MakeGenericType(underlyingType);

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