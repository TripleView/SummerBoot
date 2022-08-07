using System;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

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

        /// <summary>
        /// 转换为base64编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToBase64(this string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            return bytes.ToBase64();
        }

        /// <summary>
        /// 转为base64编码
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToBase64(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }
        /// <summary>
        /// 使用sha1计算散列值
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToSha1(this string str)
        {
            SHA1 sha1 = SHA1.Create();
            byte[] originalPwd = Encoding.UTF8.GetBytes(str);
            //加密
            byte[] newPwd = sha1.ComputeHash(originalPwd);
            var result = string.Join("", newPwd.Select(o => string.Format("{0:x2}", o)).ToArray());
            return result;
        }

        /// <summary>
        /// 使用sha256计算散列值
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToSha256(this string str)
        {
            byte[] originalPwd = Encoding.UTF8.GetBytes(str);
            return originalPwd.ToSha256();
        }

        /// <summary>
        /// 使用sha256计算散列值
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToSha256(this byte[] bytes)
        {
            var sha1 = SHA256.Create();
            //加密
            byte[] newPwd = sha1.ComputeHash(bytes);
            var result = string.Join("", newPwd.Select(o => string.Format("{0:x2}", o)).ToArray());
            return result;
        }
        /// <summary>
        /// 转为md5字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToMd5(this string str)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(str);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 先使用Sha256再使用base64
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToSha256ThenBase64(this byte[] bytes)
        {
            var sha1 = SHA256.Create();
            //加密
            byte[] newPwd = sha1.ComputeHash(bytes);
            var result = newPwd.ToBase64();
            return result;
        }


        /// <summary>
        /// 先使用Sha1再使用base64
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToSha1ThenBase64(this byte[] bytes)
        {
            var sha1 = SHA1.Create();
            //加密
            byte[] newPwd = sha1.ComputeHash(bytes);
            var result = newPwd.ToBase64();
            return result;
        }
        /// <summary>
        /// 先使用Sha1再使用base64
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToSha1ThenBase64(this string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            return bytes.ToSha1ThenBase64();
        }
        /// <summary>
        /// 先转为HmacSha256再使用base64
        /// </summary>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string ToHmacSha256ThenBase64(this string source, byte[] key)
        {
            var hmaCmd = new HMACSHA256(key);
            byte[] bytes = hmaCmd.ComputeHash(Encoding.UTF8.GetBytes(source));
            var result = bytes.ToBase64();
            hmaCmd.Clear();
            return result;
        }

        public static byte[] GetBytes(this string source, Encoding encoding)
        {
            return encoding.GetBytes(source);
        }

        /// <summary>
        /// 转为16进制
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToHex(this byte[] bytes)
        {
            StringBuilder ret = new StringBuilder();
            foreach (byte bx in bytes)
            {
                //{0:X2} 大写
                ret.AppendFormat("{0:x2}", bx);
            }
            var hex = ret.ToString();
            return hex;
        }

        public static string ToHex(this string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            return bytes.ToHex();
        }

        public static byte[] ToBytes(this int value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Return an array of bytes representing an integer.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="len">Length of bytes object to use. An OverflowError is raised if the integer is not representable with the given number of bytes.</param>
        /// <returns></returns>
        public static byte[] ToBytesBig(this int number, int len)
        {
            byte[] bytes = BitConverter.GetBytes(number);
            if (BitConverter.IsLittleEndian)                //if little endian, reverse to get big endian
                Array.Reverse(bytes);
            if (bytes.Length == len) return bytes;          //if already desired length, return.
            if (bytes.Length > len)                         //if length is too long, remove some elements
            {
                var bytesTmp = Array.Empty<byte>();
                bytes.CopyTo(bytesTmp, bytes.Length - len);
                bytes = bytesTmp;
            }
            else                                            //if length is too small, add 0's in byte
            {
                Array.Reverse(bytes);
                for (var i = bytes.Length; i < len; i++)
                    bytes[i] = (byte)0;
                Array.Reverse(bytes);
            }
            return bytes;
        }
    }
}