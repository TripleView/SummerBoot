using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Linq;
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
    }
}