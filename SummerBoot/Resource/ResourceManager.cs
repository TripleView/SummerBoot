using System.Globalization;
using System.Runtime.CompilerServices;

namespace SummerBoot.Resource
{
    internal static class ResourceManager
    {
        public static CultureInfo cultureInfo;

        public static string Get(string key)
        {
            var manager = new System.Resources.ResourceManager("SummerBoot", typeof(ResourceManager).Assembly);
            return manager.GetString(key, cultureInfo);
        }
    }
}