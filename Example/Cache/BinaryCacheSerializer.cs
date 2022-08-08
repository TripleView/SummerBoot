using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SummerBoot.Cache;

namespace Example.Cache
{
    public class BinaryCacheSerializer : ICacheSerializer
    {
        public object SerializeObject<T>(T obj)
        {
            using (var stream=new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream,obj);
                var result = stream.ToArray();
                return result;
            }
        }
    }
}