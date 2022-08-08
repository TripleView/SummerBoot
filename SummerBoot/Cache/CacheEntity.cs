using System;

namespace SummerBoot.Cache
{
    [Serializable]
    public class CacheEntity<T>
    {
        public bool HasValue { get; set; }

        public T Data { get; set; }
    }
}