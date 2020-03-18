using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;

namespace SummerBoot.Core
{
    public static partial class SbUtil
    {
        /// <summary>
        /// 流转化为字符串
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string ConvertToString(this Stream stream)
        {
            var streamReader=new StreamReader(stream);
            return streamReader.ReadToEnd();
        }

        /// <summary>
        /// 判断2个流是否相等
        /// </summary>
        /// <param name="content"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool Matches(this Stream content, Stream target)
        {
            using var sha256 = SHA256.Create();
            var contentHash = sha256.ComputeHash(content);
            var targetHash = sha256.ComputeHash(target);

            return contentHash.SequenceEqual(targetHash);
        }
    }


}