using System;
using System.Collections.Generic;

namespace SummerBoot.Core
{
    public static partial class SbUtil
    {
        /// <summary>
        /// 获取枚举所有值列表
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static List<KeyValuePair<int, string>> GetKeyValueList(this Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new NotSupportedException("must be enum");
            }
            var result = new List<KeyValuePair<int, string>>();
            var enumList = Enum.GetValues(enumType);
            foreach (var o in enumList)
            {
                var key = (int)o;
                var value = o.ToString();
                var item = new KeyValuePair<int, string>(key, value);
                result.Add(item);
            }

            return result;
        }
    }
}