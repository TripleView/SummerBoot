using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SummerBoot.Core
{
    public static partial class SbUtil
    {
        public static bool IsNullOrEmpty(this IEnumerable enumerable)
        {
            if (enumerable != null)
                return !enumerable.GetEnumerator().MoveNext();
            return true;
        }

        public static bool IsNotNullAndNotEmpty<T>(this List<T> list)
        {
            return list != null && list.Count > 0;
        }

        /// <summary>
        /// 对字符串集合进行拼接
        /// </summary>
        /// <param name="source"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string StringJoin(this IEnumerable<string> source,char separator= ',')
        {
           return string.Join(separator, source);
        }
    }
}