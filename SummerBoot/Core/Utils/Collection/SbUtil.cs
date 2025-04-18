using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SummerBoot.Core
{
    public static partial class SbUtil
    {
        public static bool IsNullOrEmpty(this IEnumerable enumerable)
        {
            if (enumerable != null)
                return !enumerable.GetEnumerator().MoveNext();
            return true;
        }

        public static bool IsNotNullAndNotEmpty<T>(this List<T> list)
        {
            return list != null && list.Count > 0;
        }

        /// <summary>
        /// Concatenate a collection of strings;对字符串集合进行拼接
        /// </summary>
        /// <param name="source"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string StringJoin(this IEnumerable<string> source, char separator = ',')
        {
            return string.Join(separator, source);
        }
        /// <summary>
        /// Concatenate a collection of strings;对字符串集合进行拼接
        /// </summary>
        /// <param name="source"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string StringJoin(this IEnumerable<string> source, string separator = "")
        {
            return string.Join(separator, source);
        }

        /// <summary>
        /// Get in batches;分批次获取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="pList"></param>
        /// <param name="func"></param>
        /// <param name="batchCount"></param>
        /// <returns></returns>
        public static async Task<List<T>> GetBatchDataAsync<T, T1>(this List<T1> pList, Func<List<T1>, Task<List<T>>> func, int batchCount = 500)
        {
            var resultList = new List<T>();
            var tempParameterList = new List<T1>();
            for (int i = 0; i < pList.Count; i++)
            {
                tempParameterList.Add(pList[i]);
                if (i != 0 && tempParameterList.Count % batchCount == 0 || i == pList.Count - 1)
                {
                    var tempResultList = await func(tempParameterList);
                    resultList.AddRange(tempResultList);
                    tempParameterList.Clear();
                }
            }

            return resultList;
        }

        /// <summary>
        /// Get in batches;分批次获取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="pList"></param>
        /// <param name="func"></param>
        /// <param name="batchCount"></param>
        /// <returns></returns>
        public static List<T> GetBatchData<T, T1>(this List<T1> pList, Func<List<T1>, List<T>> func, int batchCount = 500)
        {
            var resultList = new List<T>();
            var tempParameterList = new List<T1>();
            for (int i = 0; i < pList.Count; i++)
            {
                tempParameterList.Add(pList[i]);
                if (i != 0 && tempParameterList.Count % batchCount == 0 || i == pList.Count - 1)
                {
                    var tempResultList = func(tempParameterList);
                    resultList.AddRange(tempResultList);
                    tempParameterList.Clear();
                }
            }

            return resultList;
        }

        /// <summary>
        /// Execute in batches;分批次执行
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="pList"></param>
        /// <param name="action"></param>
        /// <param name="batchCount"></param>
        /// <returns></returns>
        public static async Task BatchExecutionAsync<T1>(this List<T1> pList, Func<List<T1>, Task> action, int batchCount = 500)
        {
            var tempParameterList = new List<T1>();
            for (int i = 0; i < pList.Count; i++)
            {
                tempParameterList.Add(pList[i]);
                if (i != 0 && tempParameterList.Count % batchCount == 0 || i == pList.Count - 1)
                {
                    await action(tempParameterList);
                    tempParameterList.Clear();
                }
            }
        }

        /// <summary>
        /// Execute in batches;分批次执行
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="pList"></param>
        /// <param name="action"></param>
        /// <param name="batchCount"></param>
        /// <returns></returns>
        public static void BatchExecution<T1>(this List<T1> pList, Action<List<T1>> action, int batchCount = 500)
        {
            var tempParameterList = new List<T1>();
            for (int i = 0; i < pList.Count; i++)
            {
                tempParameterList.Add(pList[i]);
                if (i != 0 && tempParameterList.Count % batchCount == 0 || i == pList.Count - 1)
                {
                    action(tempParameterList);
                    tempParameterList.Clear();
                }
            }
        }
    }
}