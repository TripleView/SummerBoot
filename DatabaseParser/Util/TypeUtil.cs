using System;

namespace DatabaseParser.Util
{
    public static class TypeUtil
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
    }
}