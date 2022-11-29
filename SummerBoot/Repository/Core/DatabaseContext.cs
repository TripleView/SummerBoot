using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using SummerBoot.Core;
using SummerBoot.Core.Utils.Reflection;
using YamlDotNet.Core.Tokens;

namespace SummerBoot.Repository.Core
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    public static class DatabaseContext
    {
      

        //通过索引获取值
        private static readonly MethodInfo GetItem = typeof(IDataRecord).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(it =>
                it.Name == "get_Item" && it.GetParameters().Length == 1 &&
                it.GetParameters()[0].ParameterType == typeof(int));

        private static object lockObj = new object();

        /// <summary>
        /// 读取dataReader转化为type类型
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dr"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Func<IDataReader, object> GetTypeDeserializer(Type type, IDataReader dr,DatabaseUnit databaseUnit)
        {
            var dynamicMethod = new DynamicMethod("GetTypeDeserializer" + Guid.NewGuid().ToString("N"), typeof(object),
                new Type[] { typeof(IDataReader) });
            var il = dynamicMethod.GetILGenerator();
            var errorIndexLocal = il.DeclareLocal(typeof(int));
            var returnValueLocal = il.DeclareLocal(type);
           var tyrLabel= il.BeginExceptionBlock();//try
                                     //定义返回值


            if (type.IsValueType)
            {
                il.Emit(OpCodes.Ldloca, returnValueLocal);
                il.Emit(OpCodes.Initobj, type);
                il.Emit(OpCodes.Ldloca, returnValueLocal);
            }
            else
            {
                var ctor = type.GetConstructor(Type.EmptyTypes);
                if (ctor == null)
                {
                    throw new Exception($"Type:{type.FullName} must have a parameterless constructor");
                }
                il.Emit(OpCodes.Newobj, ctor);
                il.Emit(OpCodes.Stloc, returnValueLocal);
                il.Emit(OpCodes.Ldloc, returnValueLocal);
            }

            //堆栈用[]表示，则当前为type的实例，即[target]

            var queryMemberCacheInfos = GetQueryMemberCacheInfos(dr, type);

            if (queryMemberCacheInfos.Count == 0)
            {
                throw new Exception("The column to query could not be found");
            }
            var backUpObject = il.DeclareLocal(typeof(object));
            var endLabel = il.DefineLabel();
            // 
            for (var i = 0; i < queryMemberCacheInfos.Count; i++)
            {
                var queryMemberCacheInfo = queryMemberCacheInfos[i];
                var drFieldType = dr.GetFieldType(queryMemberCacheInfo.DataReaderIndex);
                var entityFieldType = queryMemberCacheInfo.PropertyInfo.PropertyType;
                var nullableEntityFieldType = Nullable.GetUnderlyingType(entityFieldType);
                //实际类型
                var realType = nullableEntityFieldType ?? entityFieldType;
                
                var dbNullLabel= il.DefineLabel();
                var finishLabel = il.DefineLabel();
                il.Emit(OpCodes.Dup);// [target,target]
                il.EmitInt32(i);// [target,target,i]
                il.SteadOfLocal(errorIndexLocal);//[target,target]
                //通过索引从dataReader里读取数据，此时读取回来的是object类型
                il.Emit(OpCodes.Ldarg_0); //[target, target,dataReader]
                il.EmitInt32(queryMemberCacheInfo.DataReaderIndex);//[target, target,dataReader,i]
                il.Emit(OpCodes.Callvirt, GetItem);// [target, target, getItemValue]
                //判断返回值是否为dbnull，如果是，则跳转到结束
                il.Emit(OpCodes.Dup);// [target, target, getItemValue,getItemValue]
                il.Emit(OpCodes.Isinst, typeof(DBNull));// [target, target, getItemValue, bool]
                il.Emit(OpCodes.Brtrue_S, dbNullLabel);// [target, target, getItemValue]
                //il.Emit(OpCodes.Call, typeof(DatabaseContext).GetMethod(nameof(DebugObj)));
                //对获取到的值进行备份,存到字段backUpObject里
                il.Emit(OpCodes.Dup);// [target, target, getItemValue,getItemValue]
                il.SteadOfLocal(backUpObject);// [target, target, getItemValue]

                if (DatabaseUnit.TypeHandlers[databaseUnit.Id].ContainsKey(realType))
                {
                    //这里是一个静态类，可以直接调用。
                    var typeHandlerCacheType = DatabaseUnit.TypeHandlers[databaseUnit.Id][realType];
                    var c = typeHandlerCacheType.GetMethod("Parse");
                    il.Emit(OpCodes.Call, typeHandlerCacheType.GetMethod("Parse"));
                    //if (nullableEntityFieldType != null)
                    //{

                    //}
                    //else
                    //{
                    //    il.Emit(OpCodes.Unbox_Any, realType);
                    //}
                    
                }
                else
                {
                    il.ConvertTypeToTargetType(drFieldType, entityFieldType);// [target, target, realValue]
                }
               
                il.Emit(OpCodes.Call, queryMemberCacheInfo.PropertyInfo.GetSetMethod()); //[target]
                il.Emit(OpCodes.Br_S, finishLabel);


                // il.Emit(OpCodes.Call,typeof(object).GetMethod(nameof(object.ToString),new []{typeof(string)}));
                // il.Emit(OpCodes.Box,typeof(object));
                // il.Emit(OpCodes.Call,typeof(DatabaseContext).GetMethod(nameof(DebugObj)));
                // il.Emit(OpCodes.Call,typeof(Console).GetMethod(nameof(Console.WriteLine),new []{typeof(object)}));

                il.MarkLabel(dbNullLabel);
                il.Emit(OpCodes.Pop);// [target, target]
                il.Emit(OpCodes.Pop);// [target]
                il.Emit(OpCodes.Br_S, finishLabel);
                il.MarkLabel(finishLabel);
            }
            il.MarkLabel(endLabel);
            il.SteadOfLocal(returnValueLocal);

            il.BeginCatchBlock(typeof(Exception));
            //此时栈顶元素为[exception]
            il.Emit(OpCodes.Call, typeof(DatabaseContext).GetMethod(nameof(ThrowRepositoryException)));
            il.EndExceptionBlock();

            il.Emit(OpCodes.Ldloc, returnValueLocal);
            il.Emit(OpCodes.Ret);

            var result = (Func<IDataReader, object>)dynamicMethod.CreateDelegate(typeof(Func<IDataReader, object>));

            return result;
        }


        public static void DebugBreakPoint()
        {
            var st = new StackTrace();
        }
        public static void DebugObj(object obj)
        {
            var st = new StackTrace();
        }
        public static void ThrowRepositoryException(Exception ex)
        {

        }


        /// <summary>
        /// 获取要查询的属性列表
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static List<MemberInfoCache> GetQueryMemberCacheInfos(IDataReader dr, Type type)
        {
            var memberCacheInfos = type.GetMemberInfoCachesForSetting();
            var tableColNames = Enumerable.Range(0, dr.FieldCount).Select(it => new {name=dr.GetName(it),index=it}).ToList();
            var result = new List<MemberInfoCache>();
            foreach (var info in memberCacheInfos)
            {
                var tableColName = tableColNames.FirstOrDefault(it =>
                    string.Equals(it.name, info.Name, StringComparison.InvariantCultureIgnoreCase));
                if (tableColName == null)
                {
                    continue;
                }

                info.DataReaderIndex = tableColName.index;
                result.Add(info);
            }
  
            return result;
        }


        /// <summary>
        /// 给请求设置参数
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        public static void SetParameters(IDbCommand cmd, DynamicParameters parameters)
        {
           var databaseType=  cmd.Connection.GetDatabaseType();
           var paramInfos= parameters.GetParamInfos;

        }


        /// <summary>
        /// 获取数据库类型
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static DatabaseType GetDatabaseType(this IDbConnection dbConnection)
        {
            var dbConnectionType = dbConnection.GetType();
            var dbName = dbConnectionType.FullName;
            if (dbName.ToLower().IndexOf("sqlite") > -1)
            {
                return DatabaseType.Sqlite;
            }else if (dbName.ToLower().IndexOf("mysql") > -1)
            {
                return DatabaseType.Mysql;
            }else if (dbName.ToLower().IndexOf("sqlconnection") > -1 && dbName.ToLower().IndexOf("microsoft") > -1
                      || (dbName.ToLower().IndexOf("sqlconnection") > -1 && dbName.ToLower().IndexOf("system") > -1))
            {
                return DatabaseType.SqlServer;
            }
            else if(dbName.ToLower().IndexOf("oracle") > -1)
            {
                return DatabaseType.Oracle;
            }
            else
            {
                throw new NotSupportedException(nameof(dbName));
            }
        }

        /// <summary>
        /// 动态生成TypeHandlerCache类
        /// </summary>
        /// <returns></returns>
        public static Type GenerateTypeHandlerCacheClass(Type defineType)
        {
            var name = "ITest";
            string assemblyName = name + "ProxyAssembly";
            string moduleName = name + "ProxyModule";
            string typeName = name + "Proxy";

            AssemblyName assyName = new AssemblyName(assemblyName);
            AssemblyBuilder assyBuilder = AssemblyBuilder.DefineDynamicAssembly(assyName, AssemblyBuilderAccess.Run);
            ModuleBuilder modBuilder = assyBuilder.DefineDynamicModule(moduleName);
            //新类型的属性
            TypeAttributes newTypeAttribute = TypeAttributes.Public | TypeAttributes.Abstract |
                                              TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit |
                                              TypeAttributes.Class;

            //父类型
            Type parentType;
            //要实现的接口
            Type[] interfaceTypes = Type.EmptyTypes;
            parentType = null;

            //得到类型生成器            
            TypeBuilder typeBuilder = modBuilder.DefineType("GenerateTypeHandlerCacheClass", newTypeAttribute, parentType, interfaceTypes);

            var typeParams = typeBuilder.DefineGenericParameters("T");

            GenericTypeParameterBuilder first = typeParams[0];
            //first.SetGenericParameterAttributes(
            //    GenericParameterAttributes.ReferenceTypeConstraint);

            var staticTypeHandlerField = typeBuilder.DefineField("handler", typeof(ITypeHandler<>).MakeGenericType(defineType),
                FieldAttributes.Public | FieldAttributes.Static);

            //SetValue方法
            var staticSetValueMethod = typeBuilder.DefineMethod("SetValue", MethodAttributes.Static | MethodAttributes.Public,
                CallingConventions.Standard, null, new Type[] { typeof(IDbDataParameter), defineType });
            var ilGenerator = staticSetValueMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldsfld, staticTypeHandlerField);
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Callvirt, typeof(ITypeHandler<>).MakeGenericType(defineType).GetMethod("SetValue"));
            ilGenerator.Emit(OpCodes.Ret);

            //SetHandler方法
            var staticSetMethod = typeBuilder.DefineMethod("SetHandler", MethodAttributes.Static | MethodAttributes.Public,
                CallingConventions.Standard, null, new Type[] { typeof(ITypeHandler<>).MakeGenericType(defineType) });
            var ilG2 = staticSetMethod.GetILGenerator();
            ilG2.Emit(OpCodes.Ldarg_0);
            ilG2.Emit(OpCodes.Stsfld, staticTypeHandlerField);
            ilG2.Emit(OpCodes.Ret);

            //Parse方法
            var staticParseMethod = typeBuilder.DefineMethod("Parse", MethodAttributes.Static | MethodAttributes.Public,
                CallingConventions.Standard, defineType, new Type[] { typeof(object) });
            var ilParseGenerator = staticParseMethod.GetILGenerator();
            ilParseGenerator.Emit(OpCodes.Ldsfld, staticTypeHandlerField);
            ilParseGenerator.Emit(OpCodes.Ldarg_0);
            ilParseGenerator.Emit(OpCodes.Callvirt, typeof(ITypeHandler<>).MakeGenericType(defineType).GetMethod("Parse"));
            ilParseGenerator.Emit(OpCodes.Ret);

            var resultType = typeBuilder.CreateTypeInfo().AsType();
            resultType = resultType.MakeGenericType(defineType);
            return resultType;
        }
    }
}
