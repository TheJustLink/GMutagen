using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serialization.Attributes;

namespace Serialization.Resolvers;

public class SerializeAttributeContractResolver : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        return base.CreateProperties(type, memberSerialization)
            .Where(property => HasSerializeAttribute(type, property.UnderlyingName))
            .ToList();
    }

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);
        var hasAttribute = member.GetCustomAttribute<SerializeAttribute>() != null;
        
        property.ShouldSerialize = _ => hasAttribute;
        property.Ignored = !hasAttribute;
        
        return property;
    }

    private static bool HasSerializeAttribute(Type type, string memberName)
    {
        var member = type.GetMember(memberName).FirstOrDefault();
        return member?.GetCustomAttribute<SerializeAttribute>() != null;
    }
}