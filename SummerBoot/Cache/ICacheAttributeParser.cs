using System.Collections.Generic;
using System.Reflection;

namespace SummerBoot.Cache
{
    /// <summary>
    /// 缓存注解解析接口
    /// </summary>
    public interface ICacheAttributeParser
    {
        /// <summary>
        ///解析注解，获得缓存操作封装类
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        ICollection<CacheOperation> ParseCacheAttributes(MethodInfo methodInfo);
    }
}