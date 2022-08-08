namespace SummerBoot.Cache
{
    public class CacheOption
    {
        public ICacheDeserializer CacheDeserializer { get; set; }
        public ICacheSerializer CacheSerializer { get; set; }


    }
}