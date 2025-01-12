using System.Collections.Concurrent;
using SummerBoot.Feign.Nacos.Dto;

namespace SummerBoot.Feign.Nacos
{
    public class NacosUtil
    {


        public static string LINE_SEPARATOR = char.ToString((char)1);

        public static string WORD_SEPARATOR = char.ToString((char)2);

        public static string CONFIG_TYPE = "Config-Type";

        public static string GetServiceName(string groupName, string serviceName)
        {
            return groupName + "@@" + serviceName;
        }

        public static ConcurrentDictionary<string, FeignCacheEntity> FeignCache = new ConcurrentDictionary<string, FeignCacheEntity>();
    }


}