using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GMutagen.v8.Test;

public static class CustomAttributeDataExtensions
{
    public static bool Contains<T>(this IEnumerable<CustomAttributeData> attributes)
    {
        return attributes.Any(attribute => attribute.AttributeType.IsAssignableTo(typeof(T)));
    }
    public static CustomAttributeData? Get<T>(this IEnumerable<CustomAttributeData> attributes)
    {
        return attributes.FirstOrDefault(attribute => attribute.AttributeType.IsAssignableTo(typeof(T)));
    }
}