using System.Collections.Generic;
using SummerBoot.Core;

namespace SummerBoot.Cache
{
    public interface ICacheManager : IInitializing
    {
        string GetName();
        /// <summary>
        /// 根据Cache名字获取Cache  
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        ICache GetCache(string name);

        /// <summary>
        /// 得到所有Cache的名字  
        /// </summary>
        /// <returns></returns>
        IList<string> GetCacheNames();
    }
}