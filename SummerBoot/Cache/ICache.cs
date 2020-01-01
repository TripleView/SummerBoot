using System;
using SummerBoot.Core;

namespace SummerBoot.Cache
{
    public interface ICache
    {
        /// <summary>
        /// 缓存的名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 获得真实的缓存
        /// </summary>
        object GetNativeCache { get; }

        /// <summary>
        /// 根据key获得值，如果值不存在，则回调Action，获得值以后put进缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="callBack">回调函数</param>
        /// <returns></returns>
        T Get<T>(string key, Func<T> callBack = null);

        IValueWrapper Get(string key, Type returnType, Func<object> callBack = null);

        /// <summary>
        /// 加入缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Put(string key, object value);

        /// <summary>
        /// 根据key移除缓存
        /// </summary>
        /// <param name="key"></param>
        void Evict(string key);

        /// <summary>
        /// 清空缓存
        /// </summary>
        void Clear();
    }
}