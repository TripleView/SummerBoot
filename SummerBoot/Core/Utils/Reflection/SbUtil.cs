using System;
using System.Collections;
using System.Linq;

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
            return type.GetInterfaces().Any(it => it == typeof(IEnumerable));
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
        /// 根据类名获得Type实例
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type ForName(string typeName)
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
    }
}