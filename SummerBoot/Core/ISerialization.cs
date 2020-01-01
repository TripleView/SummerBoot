using System;

namespace SummerBoot.Core
{
    public interface ISerialization
    {
        string SerializeObject(object obj);

        object DeserializeObject(string value, Type type);
    }
}