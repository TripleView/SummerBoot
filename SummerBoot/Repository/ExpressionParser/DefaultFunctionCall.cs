using System;
using System.Reflection;

namespace SummerBoot.Repository.ExpressionParser;

public static class DefaultFunctionCall
{
    public static MethodInfo Trim = typeof(string).GetMethod(nameof(string.Trim), new Type[] { });
    public static MethodInfo TrimStart = typeof(string).GetMethod(nameof(string.TrimStart), new Type[] { });
    public static MethodInfo TrimEnd = typeof(string).GetMethod(nameof(string.TrimEnd), new Type[] { });
    public static MethodInfo ToUpper = typeof(string).GetMethod(nameof(string.ToUpper), new Type[] { });
    public static MethodInfo ToUpperInvariant = typeof(string).GetMethod(nameof(string.ToUpperInvariant), new Type[] { });
    public static MethodInfo ToLower = typeof(string).GetMethod(nameof(string.ToLower), new Type[] { });
    public static MethodInfo ToLowerInvariant = typeof(string).GetMethod(nameof(string.ToLowerInvariant), new Type[] { });
    public static MethodInfo StartsWith = typeof(string).GetMethod(nameof(string.StartsWith), new Type[] { typeof(string) });
    public static MethodInfo Contains = typeof(string).GetMethod(nameof(string.Contains), new Type[] { typeof(string) });
    public static MethodInfo EndsWith = typeof(string).GetMethod(nameof(string.EndsWith), new Type[] { typeof(string) });
    public static MethodInfo Equals = typeof(string).GetMethod(nameof(string.Equals), new Type[] { typeof(string) });
}