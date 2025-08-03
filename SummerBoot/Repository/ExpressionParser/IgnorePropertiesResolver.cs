using System.Collections.Generic;

namespace SummerBoot.Repository.ExpressionParser;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

public class IgnorePropertiesResolver : DefaultContractResolver
{
    private readonly HashSet<string> _ignoredProperties;

    public IgnorePropertiesResolver(IEnumerable<string> ignoredProperties)
    {
        _ignoredProperties = new HashSet<string>(ignoredProperties);
    }

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);
        // ��������ں����б��У������л�
        if (_ignoredProperties.Contains(property.PropertyName))
        {
            property.ShouldSerialize = instance => false;
        }
        return property;
    }
}