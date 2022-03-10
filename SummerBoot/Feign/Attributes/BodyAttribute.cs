using System;

namespace SummerBoot.Feign.Attributes
{
    /// <summary>
    /// post请求载荷注解
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class BodyAttribute : Attribute
    {
        public BodyAttribute(BodySerializationKind serializationKind = BodySerializationKind.Json)
        {
            this.SerializationKind = serializationKind;
        }
        public BodySerializationKind SerializationKind { get; set; }
    }
}
