using System;
using System.Collections;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;

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
    }
}