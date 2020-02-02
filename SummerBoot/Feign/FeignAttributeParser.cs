using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace SummerBoot.Feign
{
    public class FeignAttributeParser : IFeignAttributeParser
    {
        public ICollection<HttpOperation> ParseFeignAttributes(MethodInfo methodInfo)
        {
            ICollection<HttpOperation> result = new List<HttpOperation>();

            //所有注解
            var attributes = methodInfo.GetCustomAttributes().ToList();
            //解析get注解
            attributes.OfType<GetMappingAttribute>().ToList().ForEach(it => result.Add(ParseHttpGetAttribute(it)));
            //解析post注解
            attributes.OfType<PostMappingAttribute>().ToList().ForEach(it => result.Add(ParseHttpPostAttribute(it)));
          
            return result;
        }

        /// <summary>
        /// //解析读取注解,封装成缓存读取类
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        private HttpGetOperation ParseHttpGetAttribute(GetMappingAttribute attribute)
        {
            return new HttpGetOperation(attribute.Value);
        }

        /// <summary>
        /// //解析添加注解,封装成缓存添加类
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        private HttpPostOperation ParseHttpPostAttribute(PostMappingAttribute attribute)
        {
            return new HttpPostOperation(attribute.Value);
        }

    }
}