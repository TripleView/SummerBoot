using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;

namespace SummerBoot.Core
{
    public static partial class SbUtil
    {
        /// <summary>
        /// 判断字符串是否为空
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// 判断字符串是否有值
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasText(this string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// 字符串转byte数组
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] GetBytes(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        /// <summary>
        /// byte数组获得字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string GetString(this byte [] bytes)
        {
            if (bytes == null) return string.Empty;
            return Encoding.UTF8.GetString(bytes);
        }
    }
}