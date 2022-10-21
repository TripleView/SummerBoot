using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using SummerBoot.Core;

namespace SummerBoot.Repository.Core
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    public static class DatabaseContext
    {
        /// <summary>
        /// 缓存Type中提取出来的属性信息
        /// </summary>
        private static ConcurrentDictionary<Type, List<MemberCacheInfo>> typeInfoCache =
            new ConcurrentDictionary<Type, List<MemberCacheInfo>>();

        //通过索引获取值
        private  static readonly MethodInfo GetItem = typeof(IDataRecord).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(it =>
                it.Name == "get_Item" && it.GetParameters().Length == 1 &&
                it.GetParameters()[0].ParameterType == typeof(int));

        private static object lockObj = new object();

        public static Func<IDataReader, object> GetTypeDeserializer(Type type, IDataReader dr)
        {
            var dynamicMethod = new DynamicMethod("GetTypeDeserializer" + Guid.NewGuid().ToString("N"), typeof(object),
                new Type[]{typeof(IDataReader)});
            var il = dynamicMethod.GetILGenerator();
            var errorIndexLocal = il.DeclareLocal(typeof(int));
            il.BeginExceptionBlock();//try
            //定义返回值
            var returnValueLocal = il.DeclareLocal(type);
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Ldloca, returnValueLocal);
                il.Emit(OpCodes.Initobj,type);
                il.Emit(OpCodes.Ldloca, returnValueLocal);
            }
            else
            {
                var ctor=type.GetConstructor(Type.EmptyTypes);
                if (ctor == null)
                {
                    throw new Exception($"Type:{type.FullName} must have a parameterless constructor");
                }
                il.Emit(OpCodes.Newobj,ctor);
                il.Emit(OpCodes.Stloc,returnValueLocal);
                il.Emit(OpCodes.Ldloc, returnValueLocal);
            }

            //堆栈用[]表示，则当前为type的实例，即[target]

            var queryMemberCacheInfos = GetQueryMemberCacheInfos(dr, type);

            if (queryMemberCacheInfos.Count == 0)
            {
                throw new Exception("The column to query could not be found");
            }
            var backUpObject = il.DeclareLocal(typeof(object));
            //var endLabel = il.DefineLabel();
            //for (var i = 0; i < queryMemberCacheInfos.Count; i++)
            //{
            //    var queryMemberCacheInfo = queryMemberCacheInfos[i];

            //    il.Emit(OpCodes.Dup);// [target,target]
            //    il.EmitInt32(i);// [target,target,i]
            //    il.SteadOfLocal(errorIndexLocal);//[target,target]
            //    //通过索引从dataReader里读取数据，此时读取回来的是object类型
            //    il.Emit(OpCodes.Ldarg_0); //[target, target,dataReader]
            //    il.EmitInt32(i);//[target, target,dataReader,i]
            //    il.Emit(OpCodes.Callvirt,GetItem);// [target, target, getItemValue]
            //    //对获取到的值进行备份,存到字段backUpObject里
            //    il.Emit(OpCodes.Dup);// [target, target, getItemValue,getItemValue]
            //    il.Emit(OpCodes.Stloc,backUpObject);// [target, target, getItemValue]

            //}
            il.BeginCatchBlock(typeof(Exception));

            il.EndExceptionBlock();

            il.Emit(OpCodes.Ldstr,"何泽平");
            il.Emit(OpCodes.Ret);

            var result= (Func<IDataReader,object>)dynamicMethod.CreateDelegate(typeof(Func<IDataReader, object>));

            return result;
        }



        /// <summary>
        /// 获取要查询的属性列表
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static List<MemberCacheInfo> GetQueryMemberCacheInfos(IDataReader dr,Type type)
        {
            var memberCacheInfos = GetMemberCacheInfos(type);
            var tableColNames = Enumerable.Range(0, dr.FieldCount).Select(it => dr.GetName(it)).ToList();
            var result = memberCacheInfos.Where(it =>
                tableColNames.Any(x => string.Equals(x, it.Name, StringComparison.InvariantCultureIgnoreCase))).ToList();
            return memberCacheInfos;
        }

        /// <summary>
        /// 获取类型里的属性列表，仅支持属性
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static List<MemberCacheInfo> GetMemberCacheInfos(Type type)
        {
            if (typeInfoCache.TryGetValue(type, out var result))
            {
                return result;
            }

            lock (lockObj)
            {
                if (typeInfoCache.TryGetValue(type, out result))
                {
                    return result;
                }

                result = new List<MemberCacheInfo>();

                var propertyInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(it => it.CanWrite)
                    .ToList();

                for (var i = 0; i < propertyInfos.Count; i++)
                {
                    var propertyInfo=propertyInfos[i];
                    var columnAttribute = propertyInfo.GetCustomAttribute<ColumnAttribute>();
                    var memberCacheInfo = new MemberCacheInfo()
                    {
                        Name = columnAttribute?.Name??propertyInfo.Name,
                        PropertyInfo = propertyInfo,
                        PropertyName = propertyInfo.Name
                    };
                    result.Add(memberCacheInfo);
                }

                typeInfoCache.TryAdd(type, result);
                return result;

            }
        }
    }
}
