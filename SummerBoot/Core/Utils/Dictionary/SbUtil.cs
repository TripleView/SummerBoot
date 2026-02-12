using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using YamlDotNet.Core.Tokens;

namespace SummerBoot.Core
{
    public static partial class SbUtil
    {
        /// <summary>
        /// 字典中当键不存在则添加
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool AddIfNotExist<T1, T2>(this IDictionary<T1, T2> dictionary, T1 key, T2 value)
        {
            if (!dictionary.Keys.Contains(key))
            {
                dictionary.Add(key, value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 不存在则添加，存在则替换
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool AddOrReplace<T1, T2>(this IDictionary<T1, T2> dictionary, T1 key, T2 value)
        {
            if (!dictionary.Keys.Contains(key))
            {
                dictionary.Add(key, value);
                return true;
            }
            else
            {
                dictionary[key] = value;
                return true;
            }
            return false;
        }

        public static bool AddRange<T1, T2>(this IDictionary<T1, T2> dictionary, IDictionary<T1, T2> otherDictionary)
        {
            foreach (var pair in otherDictionary)
            {
                dictionary[pair.Key] = pair.Value;
            }
            
            return true;
        }

        public static bool TryGetOrAdd<T1, T2>(this IDictionary<T1, T2> dictionary,T1 key,out T2 value,Func<T2> func)
        {
            if (dictionary.TryGetValue(key, out value))
            {
                return true;
            }

            value = func();
            dictionary.TryAdd(key, value);
            return true;
        }
    }
}