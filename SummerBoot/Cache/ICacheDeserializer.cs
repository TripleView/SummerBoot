using Newtonsoft.Json;

namespace SummerBoot.Cache
{
    public interface ICacheDeserializer
    {
        T DeserializeObject<T>(object obj);
    }

    public class JsonCacheDeserializer : ICacheDeserializer
    {
        public T DeserializeObject<T>(object obj)
        {
           var result= JsonConvert.DeserializeObject<T>(obj.ToString());
           return result;
        }
    }
}