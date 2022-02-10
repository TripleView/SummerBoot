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

        
    }
}