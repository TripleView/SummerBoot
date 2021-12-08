using System.Collections.Generic;

namespace DatabaseParser.Util
{
    public static class ListUtil
    {
        public static bool IsNotNullAndNotEmpty<T>(this List<T> list)
        {
            return list != null && list.Count > 0;
        }
    }
}