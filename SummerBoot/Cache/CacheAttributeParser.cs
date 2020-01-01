using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SummerBoot.Cache
{
    public class CacheAttributeParser : ICacheAttributeParser
    {
        public ICollection<CacheOperation> ParseCacheAttributes(MethodInfo methodInfo)
        {
            ICollection<CacheOperation> result = new List<CacheOperation>();

            //所有注解
            var attributes = methodInfo.GetCustomAttributes().ToList();
            //解析读取注解
            attributes.OfType<CacheableAttribute>().ToList().ForEach(it => result.Add(ParseCacheableAttribute(it)));
            //解析删除注解
            attributes.OfType<CacheEvictAttribute>().ToList().ForEach(it => result.Add(ParseCacheEvictAttribute(it)));
            //解析添加注解
            attributes.OfType<CachePutAttribute>().ToList().ForEach(it => result.Add(ParseCachePutAttribute(it)));
            return result;
        }

        /// <summary>
        /// //解析读取注解,封装成缓存读取类
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        private CacheableOperation ParseCacheableAttribute(CacheableAttribute attribute)
        {
            return new CacheableOperation(attribute.ToString(), attribute.CacheName, attribute.Key, attribute.Condition, attribute.KeyGenerator, attribute.CacheManager, attribute.Unless);
        }

        /// <summary>
        /// //解析添加注解,封装成缓存添加类
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        private CachePutOperation ParseCachePutAttribute(CachePutAttribute attribute)
        {
            return new CachePutOperation(attribute.ToString(), attribute.CacheName, attribute.Key, attribute.Condition, attribute.KeyGenerator, attribute.CacheManager, attribute.Unless);
        }

        /// <summary>
        /// //解析删除注解,封装成缓存删除类
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        private CacheEvictOperation ParseCacheEvictAttribute(CacheEvictAttribute attribute)
        {
            return new CacheEvictOperation(attribute.ToString(), attribute.CacheName, attribute.Key, attribute.Condition, attribute.KeyGenerator, attribute.CacheManager, attribute.AllEntries, attribute.BeforeInvocation);
        }
    }
}