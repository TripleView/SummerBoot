using Newtonsoft.Json;

namespace SummerBoot.Cache
{
    public interface ICacheSerializer
    {
        object SerializeObject<T>(T obj);
    }

    public class JsonCacheSerializer : ICacheSerializer
    {
        public object SerializeObject<T>(T obj)
        {
            var result = JsonConvert.SerializeObject(obj);
            return result;
        }
    }

}