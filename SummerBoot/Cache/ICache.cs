using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace SummerBoot.Cache
{
    public interface ICache
    {
        #region sync
        bool SetValueWithAbsolute<T>(string key, T value, TimeSpan absoluteExpiration);

        bool SetValueWithSliding<T>(string key, T value, TimeSpan slidingExpiration);

        CacheEntity<T> GetValue<T>(string key);

        bool Remove(string key);


        #endregion

        #region async

       Task<bool> SetValueWithAbsoluteAsync<T>(string key, T value, TimeSpan absoluteExpiration);

       Task<bool> SetValueWithSlidingAsync<T>(string key, T value, TimeSpan slidingExpiration);

        Task<CacheEntity<T>> GetValueAsync<T>(string key);

        Task<bool> RemoveAsync(string key);

        #endregion
    }
}