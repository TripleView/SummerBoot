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
        private static readonly ILogger Logger =new LoggerFactory().CreateLogger(typeof(SbUtil).Name);
    }
}