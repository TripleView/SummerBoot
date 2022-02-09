using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

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
        public static string GetString(this byte[] bytes)
        {
            if (bytes == null) return string.Empty;
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// 判断一个字符串里是否包含一个字符
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="value">字符</param>
        /// <returns></returns>
        public static bool HasIndexOf(this string str, char value)
        {
            return str.IndexOf(value) > -1;
        }

        /// <summary>
        /// 判断一个字符串里是否包含一个字符串
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="value">字符</param>
        /// <returns></returns>
        public static bool HasIndexOf(this string str, string value)
        {
            return str.IndexOf(value) > -1;
        }

        /// <summary>
        /// 取字符串当前值，如果当前值为空，则取默认值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static string GetValueOrDefault(this string str, string defaultValue)
        {
            return str.IsNullOrWhiteSpace() ? defaultValue : str;
        }

        /// <summary>
        /// 获取经过包装的like字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetLikeString(this string str)
        {
            if (str.HasText())
            {
                return $"%{str}%";
            }

            return str;
        }

        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static bool IsUrl(this string str)
        {
            string Url = @"^http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$";
            return Regex.IsMatch(str, Url);
        }
    }
}