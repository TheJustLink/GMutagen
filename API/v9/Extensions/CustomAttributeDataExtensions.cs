using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GMutagen.v9.Extensions;

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
    
    public static bool Contains<T>(this IEnumerable<Attribute> attributes)
    {
        return attributes.Any(attribute => attribute.GetType().IsAssignableTo(typeof(T)));
    }
    public static T? Get<T>(this IEnumerable<Attribute> attributes) where T : Attribute
    {
        return (T)attributes.FirstOrDefault(attribute => attribute.GetType().IsAssignableTo(typeof(T)));
    }
}