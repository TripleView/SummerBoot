using Newtonsoft.Json;
using SummerBoot.Core;

namespace SummerBoot.Cache
{
    public interface ICacheSerializer
    {
        byte[] SerializeObject<T>(T obj);
    }

    public class JsonCacheSerializer : ICacheSerializer
    {
        public byte[] SerializeObject<T>(T obj)
        {
            var result = JsonConvert.SerializeObject(obj);
            return result.GetBytes();
        }
    }

}