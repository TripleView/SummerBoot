using System.Collections.Generic;
using System.Reflection;

namespace SummerBoot.Feign
{
    /// <summary>
    /// Feign注解解析接口
    /// </summary>
    public interface IFeignAttributeParser
    {
        /// <summary>
        ///解析注解，获得缓存操作封装类
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        ICollection<HttpOperation> ParseFeignAttributes(MethodInfo methodInfo);
    }
}