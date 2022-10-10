using Newtonsoft.Json.Serialization;
using System.Reflection;
using SummerBoot.Feign.Attributes;
using Newtonsoft.Json;

namespace SummerBoot.Feign
{
    public class FeignContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jsonProperty = base.CreateProperty(member, memberSerialization);
            jsonProperty.PropertyName =
                member.GetCustomAttribute<AliasAsAttribute>()?.Name ?? jsonProperty.PropertyName;
            return jsonProperty;
        }
    }
}