using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Globalization;
using System.Reflection;

namespace SummerBoot.Core
{
    public static partial class SbUtil
    {
        public static MethodInfo CheckIsBoxMethod = typeof(SbUtil).GetMethod(nameof(SbUtil.CheckIsBox)),

            GetTypeFromHandleMethod = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle)),

            ConvertChangeTypeMethod = typeof(Convert).GetMethod(nameof(Convert.ChangeType), new Type[]
            {
                typeof(object), typeof(Type), typeof(IFormatProvider)
            }),

            InvariantCulture = typeof(CultureInfo).GetProperty(nameof(CultureInfo.InvariantCulture), BindingFlags.Public | BindingFlags.Static).GetGetMethod(),

            EnumParse = typeof(Enum).GetMethod(nameof(Enum.Parse), new Type[] { typeof(Type), typeof(string), typeof(bool) });
    }
}