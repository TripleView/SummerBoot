using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SummerBoot.Cache;

namespace Example.Cache
{
    public class BinaryCacheDeserializer : ICacheDeserializer
    {
        public T DeserializeObject<T>(byte[] obj)
        {
            using (var stream=new MemoryStream(obj))
            {
                stream.Seek(0, SeekOrigin.Begin);
                var result=(T)new BinaryFormatter().Deserialize(stream);
                return result;
            }
        }
    }
}