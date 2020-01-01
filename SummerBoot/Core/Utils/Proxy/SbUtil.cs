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
        public static bool IsProxy(object obj)
        {
            return ProxyUtil.IsProxy(obj);
        }
    }
}