using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using SummerBoot.Core.Utils.Reflection;
using SummerBoot.Repository;

namespace SummerBoot.Core
{
    public static partial class SbUtil
    {

        /// <summary>
        /// 判断一个类型是否为可空类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool IsNullable<T>(this T o)
        {
            var type = typeof(T);
            return type.IsNullable();
        }

        /// <summary>
        /// 判断一个类型是否为可空类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNullable(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }
        /// <summary>
        /// 判断type是否为集合类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsCollection(this Type type)
        {
            return type.GetInterfaces().Any(it => it == typeof(ICollection));
        }

        /// <summary>
        /// 判断type是否为迭代器类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsEnumerable(this Type type)
        {
            return type.GetInterfaces().Any(it => it == typeof(IEnumerable))|| type == typeof(Enumerable);
        }

        /// <summary>
        /// 判断type是否为查询器类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsQueryable(this Type type)
        {
            return type.GetInterfaces().Any(it => it == typeof(IQueryable));
        }

        /// <summary>
        /// 判断type是否为字符串类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsString(this Type type)
        {
            return type == typeof(string);
        }

        /// <summary>
        /// 判断type是否为支持async的类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsAsyncType(this Type type)
        {
            var awaiter = type.GetMethod("GetAwaiter");
            if (awaiter == null)
                return false;
            var retType = awaiter.ReturnType;
            //.NET Core 1.1及以下版本中没有 GetInterface 方法，为了兼容性使用 GetInterfaces
            if (retType.GetInterfaces().All(i => i.Name != "INotifyCompletion"))
                return false;
            if (retType.GetProperty("IsCompleted") == null)
                return false;
            if (retType.GetMethod("GetResult") == null)
                return false;

            return true;
        }

        /// <summary>
        /// 根据type 生成实例类型
        /// </summary>
        /// <param name="type"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object CreateInstance(this Type type, object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return Activator.CreateInstance(type);
            }

            return Activator.CreateInstance(type, args: args);
        }

        /// <summary>
        /// 获得基础类型，获得比如被Task，ICollection<>，IEnumable<>,IQueryable<>等包裹的类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetUnderlyingType(this Type type)
        {
            var resultTmp = type.IsAsyncType() ? type.GenericTypeArguments.First() : type;
            var resultTmp2 = resultTmp.IsGenericType
                ? resultTmp.GetGenericArguments().First()
                : resultTmp;

            return resultTmp2;
        }

        /// <summary>
        /// 判断是否为原生值类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsPrimitiveValueType(this Type type)
        {
            return type.IsValueType && type.IsPrimitive;
        }


        /// <summary>
        /// 根据类名获得Type实例
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type LoadTypeByName(string typeName)
        {
            if (typeName.IsNullOrWhiteSpace()) throw new Exception("typeName must be not empty");
            Type t = Type.GetType(typeName);
            if (t != null) System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(t.TypeHandle);
            return t;
        }

        /// <summary>
        /// 获取某个类的默认值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetDefaultValue(this Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        /// <summary>
        /// 判断是否为字典类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsDictionary(this Type type)
        {
            return type.GetInterfaces().Any(it => it == typeof(IDictionary));
        }

        /// <summary>
        /// 通过表达式树获取实体类的属性值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="model"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static TResult GetPropertyValue<T, TResult>(this T model, string propertyName)
        {
            var result = GetPropertyValue(model, propertyName);
            return (TResult)result;
        }

        public static TResult GetPropertyValueByEmit<T, TResult>(this T model, string propertyName)
        {
            var result = GetPropertyValueByEmit(model, propertyName);
            return (TResult)result;
        }

        public static ConcurrentDictionary<string, object> CacheDictionary = new ConcurrentDictionary<string, object>();
        public static ConcurrentDictionary<string, Delegate> CacheDelegateDictionary = new ConcurrentDictionary<string, Delegate>();
        /// <summary>
        /// 通过表达式树获取实体类的属性值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static object GetPropertyValue<T>(this T model, string propertyName)
        {
            var type = model.GetType();
            var property = type.GetProperty(propertyName);
            if (property == null)
            {
                throw new ArgumentNullException($"could not find property with name {propertyName}");
            }

            var key = "GetPropertyValue:" + type.FullName + property.Name;
            if (CacheDictionary.TryGetValue(key, out var func))
            {
                return ((Delegate)func).DynamicInvoke(model);
            }

            var modelExpression = Expression.Parameter(type, "model");
            var propertyExpression = Expression.Property(modelExpression, property);
            var convertExpression = Expression.Convert(propertyExpression, typeof(object));
            var lambda = Expression.Lambda(convertExpression, modelExpression).Compile();
            var result = lambda.DynamicInvoke(model);
            CacheDictionary.TryAdd(key, lambda);
            return result;
        }

        public static object GetPropertyValueByEmit<T>(this T model, string propertyName)
        {
            var type = model.GetType();
            var property = type.GetProperty(propertyName);
            
            if (property == null)
            {
                throw new ArgumentNullException($"could not find property with name {propertyName}");
            }

            var key = "GetPropertyValue2:" + type.FullName + property.Name;
            if (CacheDictionary.TryGetValue(key, out var func))
            {
                return ((Delegate)func).DynamicInvoke(model);
            }

            var dynamicMethod = new DynamicMethod("GetPropertyValueByEmit" + Guid.NewGuid().ToString("N"), typeof(object),
                new Type[] { typeof(T) });

            var il = dynamicMethod.GetILGenerator();

            if (type.IsValueType)
            {
                il.Emit(OpCodes.Ldarga_S, 0);
                il.Emit(OpCodes.Callvirt, property.GetGetMethod());
                il.Emit(OpCodes.Box, property.PropertyType);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, type);
                il.Emit(OpCodes.Callvirt, property.GetGetMethod());
                il.Emit(OpCodes.Box, property.PropertyType);
            }

            il.Emit(OpCodes.Ret);

            var lambda = dynamicMethod.CreateDelegate(typeof(Func<T, object>));
            CacheDictionary.TryAdd(key, lambda);
            var result = lambda.DynamicInvoke(model);
            return result;
        }

        /// <summary>
        /// 通过表达式树设置实体类的属性值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void SetPropertyValue<T>(this T model, string propertyName, object value)
        {
            var type = model.GetType();
            var property = type.GetProperty(propertyName);

            if (property == null)
            {
                throw new ArgumentNullException($"could not find property with name {propertyName}");
            }

            var key = "set:" + type.FullName + property.Name;
            if (CacheDictionary.TryGetValue(key, out var func))
            {
                ((Delegate)func).DynamicInvoke(model, value);
            }

            var modelExpression = Expression.Parameter(type, "model");
            var propertyExpression = Expression.Parameter(typeof(object), "val");
            var convertExpression = Expression.Convert(propertyExpression, property.PropertyType);
            var methodCallExpression = Expression.Call(modelExpression, property.GetSetMethod(), convertExpression);
            var lambda = Expression.Lambda(methodCallExpression, modelExpression, propertyExpression).Compile();
            CacheDictionary.TryAdd(key, lambda);
            lambda.DynamicInvoke(model, value);
        }

        /// <summary>
        /// 构建一个object数据转换成一维数组数据的委托
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="propertyInfos"></param>
        /// <returns></returns>
        public static Func<T, object[]> BuildObjectGetValuesDelegate<T>(List<PropertyInfo> propertyInfos) where T : class
        {
            var objParameter = Expression.Parameter(typeof(T), "model");
            var selectExpressions = propertyInfos.Select(it => BuildObjectGetValueExpression(objParameter, it));
            var arrayExpression = Expression.NewArrayInit(typeof(object), selectExpressions);
            var result = Expression.Lambda<Func<T, object[]>>(arrayExpression, objParameter).Compile();
            return result;
        }


        /// <summary>
        /// 构建对象获取单个值得
        /// </summary>
        /// <param name="modelExpression"></param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static Expression BuildObjectGetValueExpression(ParameterExpression modelExpression, PropertyInfo propertyInfo)
        {
            var propertyExpression = Expression.Property(modelExpression, propertyInfo);
            var convertExpression = Expression.Convert(propertyExpression, typeof(object));
            return convertExpression;
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> source, List<PropertyInfo> propertyInfos = null, bool useColumnAttribute = false) where T : class
        {
            var table = new DataTable("template");
            if (propertyInfos == null || propertyInfos.Count == 0)
            {
                propertyInfos = typeof(T).GetProperties().Where(it => it.CanRead).ToList();
            }
            foreach (var propertyInfo in propertyInfos)
            {
                var columnName = useColumnAttribute ? (propertyInfo.GetCustomAttribute<ColumnAttribute>()?.Name ?? propertyInfo.Name) : propertyInfo.Name;
                table.Columns.Add(columnName, ChangeType(propertyInfo.PropertyType));
            }

            Func<T, object[]> func;
            var key = typeof(T).FullName + propertyInfos.Select(it => it.Name).ToList().StringJoin(',');
            if (CacheDictionary.TryGetValue(key, out var cacheFunc))
            {
                func = (Func<T, object[]>)cacheFunc;
            }
            else
            {
                func = BuildObjectGetValuesDelegate<T>(propertyInfos);
                CacheDictionary.TryAdd(key, func);
            }

            foreach (var model in source)
            {
                var rowData = func(model);
                table.Rows.Add(rowData);
            }

            return table;
        }

        private static Type ChangeType(Type type)
        {
            if (type.IsNullable())
            {
                type = Nullable.GetUnderlyingType(type);
            }

            return type;
        }

        public static Delegate BuildGenerateObjectDelegate(ConstructorInfo constructorInfo)
        {
            var parameterExpressions = new List<ParameterExpression>();
            foreach (var parameterInfo in constructorInfo.GetParameters())
            {
                var parameterExpression = Expression.Parameter(parameterInfo.ParameterType);
                parameterExpressions.Add(parameterExpression);
            }
            var c = Expression.New(constructorInfo, parameterExpressions);
            var lambda = Expression.Lambda(c, parameterExpressions).Compile();
            return lambda;
        }

        /// <summary>
        /// 替换dataTable里的列类型
        /// </summary>
        /// <param name="dt"></param>
        public static void ReplaceDataTableColumnType<OldType, NewType>(DataTable dt, Func<OldType, NewType> replaceFunc)
        {
            var needUpdateColumnIndexList = new List<int>();
            var needUpdateColumnNameList = new List<string>();

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                var column = dt.Columns[i];
                if (column.DataType.GetUnderlyingType() == typeof(OldType))
                {
                    needUpdateColumnIndexList.Add(i);
                    needUpdateColumnNameList.Add(column.ColumnName);

                }
            }

            if (needUpdateColumnIndexList.Count == 0)
            {
                return;
            }

            var nameMapping = new Dictionary<string, string>();
            for (int i = 0; i < needUpdateColumnIndexList.Count; i++)
            {
                var oldColumnName = needUpdateColumnNameList[i];
                var newColumnName = Guid.NewGuid().ToString("N");
                nameMapping.Add(newColumnName, oldColumnName);

                dt.Columns.Add(newColumnName, typeof(byte[])).SetOrdinal(needUpdateColumnIndexList[i]);
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    var c = (dt.Rows[j][oldColumnName]);
                    dt.Rows[j][newColumnName] = replaceFunc((OldType)(dt.Rows[j][oldColumnName]));
                }
                dt.Columns.Remove(oldColumnName);
            }

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                var columnName = dt.Columns[i].ColumnName;
                if (nameMapping.ContainsKey(columnName))
                {
                    dt.Columns[i].ColumnName = nameMapping[columnName];
                }
            }

        }


        private static readonly Dictionary<Type, bool> NumberTypeDic = new Dictionary<Type, bool>()
        {
            { typeof(bool), true },
            { typeof(byte), true },
            { typeof(sbyte), true },
            { typeof(short), true },
            { typeof(ushort), true },
            { typeof(int), true },
            { typeof(uint), true },
            { typeof(long), true },
            { typeof(ulong), true },
            { typeof(float), true },
            { typeof(double), true },
            { typeof(decimal), true },
        };
        public static bool IsNumberType(this Type type)
        {
            return type.IsValueType && NumberTypeDic.ContainsKey(type);
        }

        /// <summary>
        /// 缓存Type中提取出来的可写属性信息
        /// </summary>
        private static ConcurrentDictionary<Type, List<MemberInfoCache>> settingMemberInfoCaches =
            new ConcurrentDictionary<Type, List<MemberInfoCache>>();
        /// <summary>
        /// 获取类型里的属性列表，仅支持属性
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<MemberInfoCache> GetMemberInfoCachesForSetting(this Type type)
        {
            if (settingMemberInfoCaches.TryGetValue(type, out var result))
            {
                return result;
            }

            lock (settingMemberInfoCaches)
            {
                if (settingMemberInfoCaches.TryGetValue(type, out result))
                {
                    return result;
                }

                result = new List<MemberInfoCache>();
                List<PropertyInfo> propertyInfos = new List<PropertyInfo>();

               
                if (type.IsAnonymousType())
                {
                    propertyInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(it => (it.PropertyType.IsValueType || it.PropertyType == typeof(string)))
                        .ToList();
                }
                else
                {
                    propertyInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(it => it.CanWrite && (it.PropertyType.IsValueType || it.PropertyType == typeof(string)))
                       .ToList();
                }

                for (var i = 0; i < propertyInfos.Count; i++)
                {
                    var propertyInfo = propertyInfos[i];

                    if (propertyInfo.GetIndexParameters().Length > 0)
                    {
                        continue;
                    }

                    var columnAttribute = propertyInfo.GetCustomAttribute<ColumnAttribute>();
                    var memberCacheInfo = new MemberInfoCache()
                    {
                        Name = columnAttribute?.Name ?? propertyInfo.Name,
                        PropertyInfo = propertyInfo,
                        PropertyName = propertyInfo.Name
                    };
                    result.Add(memberCacheInfo);
                }

                settingMemberInfoCaches.TryAdd(type, result);
                return result;

            }
        }

        /// <summary>
        /// 缓存Type中提取出来的可读属性信息
        /// </summary>
        private static ConcurrentDictionary<Type, List<MemberInfoCache>> gettingMemberInfoCaches =
            new ConcurrentDictionary<Type, List<MemberInfoCache>>();
        /// <summary>
        /// 获取类型里的属性列表，仅支持属性
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<MemberInfoCache> GetMemberInfoCachesForGetting(this Type type)
        {
            if (gettingMemberInfoCaches.TryGetValue(type, out var result))
            {
                return result;
            }

            lock (gettingMemberInfoCaches)
            {
                if (gettingMemberInfoCaches.TryGetValue(type, out result))
                {
                    return result;
                }

                result = new List<MemberInfoCache>();

                var propertyInfos = new List<PropertyInfo>();

                if (type.IsAnonymousType())
                {
                    propertyInfos = type.GetProperties().ToList();
                }
                else
                {
                    propertyInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(it => it.CanRead && (it.PropertyType.IsValueType || it.PropertyType == typeof(string)))
                        .ToList();
                }

                for (var i = 0; i < propertyInfos.Count; i++)
                {
                    var propertyInfo = propertyInfos[i];

                    if (propertyInfo.GetIndexParameters().Length > 0)
                    {
                        continue;
                    }
                    var columnAttribute = propertyInfo.GetCustomAttribute<ColumnAttribute>();
                    var memberCacheInfo = new MemberInfoCache()
                    {
                        Name = columnAttribute?.Name ?? propertyInfo.Name,
                        PropertyInfo = propertyInfo,
                        PropertyName = propertyInfo.Name
                    };
                    result.Add(memberCacheInfo);
                }

                gettingMemberInfoCaches.TryAdd(type, result);
                return result;

            }
        }

        /// <summary>
        /// Determine whether the type is an anonymous type(判断类型是否为匿名类)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsAnonymousType(this Type type)
        {
            return type.IsClass && type.Name.StartsWith("<>f__AnonymousType");
        }

        /// <summary>
        /// Get all classes under the current app;获取当前app下所有的类
        /// </summary>
        /// <returns></returns>
        public static List<Type> GetAppAllTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(it => !it.IsDynamic).ToList();
            var result = new List<Type>();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetExportedTypes();
                    result.AddRange(types);
                }
                catch (Exception e)
                {
                   
                }
            }

            return result;
        }
    }
}