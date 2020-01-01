using System;
using Newtonsoft.Json;

namespace SummerBoot.Core
{
    public class JsonSerialization : ISerialization
    {
        public object DeserializeObject(string value, Type type)
        {
            return JsonConvert.DeserializeObject(value, type);
        }

        public string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}