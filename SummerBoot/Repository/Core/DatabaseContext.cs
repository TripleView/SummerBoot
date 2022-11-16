using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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

        public static Func<IDataReader, object> GetTypeDeserializer(Type type, IDataReader dr)
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
                // var nullableEntityFieldType = Nullable.GetUnderlyingType(entityFieldType);
                // var unboxType = nullableEntityFieldType ?? entityFieldType;
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
                il.ConvertTypeToTargetType(drFieldType,entityFieldType);// [target, target, realValue]
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

       
    }
}
