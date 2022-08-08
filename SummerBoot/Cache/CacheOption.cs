namespace SummerBoot.Cache
{
    public class CacheOption
    {
        public ICacheDeserializer CacheDeserializer { get; set; }
        public ICacheSerializer CacheSerializer { get; set; }

        /// <summary>
        /// 使用内存缓存
        /// </summary>
        public bool UseMemory { get; set; }

        public bool UseRedis { get; set; }
        public string RedisConnectionString { get; set; }
    }
}