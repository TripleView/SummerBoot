using System;
using System.Linq;
using System.Net;
using System.Reflection;
using SummerBoot.Core;
using SummerBoot.Feign.Attributes;

namespace SummerBoot.Feign
{
    /// <summary>
    /// feign工作单元
    /// </summary>
    public interface IFeignUnitOfWork
    {
        /// <summary>
        /// 获取cookie管理器
        /// </summary>
        /// <param name="feignInterface"></param>
        /// <returns></returns>
        CookieContainer GetCookieContainer(object feignInterface);
    }

    public class DefaultFeignUnitOfWork : IFeignUnitOfWork
    {
        public CookieContainer GetCookieContainer(object feignInterfaceEntity)
        {
            var feignInterface = feignInterfaceEntity.GetType().GetInterfaces().ToList()
                .FirstOrDefault(it => it.GetCustomAttribute<FeignClientAttribute>() != null);
            var feignClient = feignInterface?.GetCustomAttribute<FeignClientAttribute>();
            if (feignClient == null)
            {
                throw new NotSupportedException("only support feignClient interface");
            }

            if (!feignClient.UseCookie)
            {
                throw new NotSupportedException("feignClient not using UseCookie=true");
            }
            var clientName = feignClient.Name.GetValueOrDefault(feignInterface.FullName);

            return SummerBootExtentions.AllClientContainerCache[clientName];
        }
    }

}