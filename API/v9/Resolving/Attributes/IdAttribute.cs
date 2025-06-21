using System;

namespace GMutagen.v9.Resolving.Attributes;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class IdAttribute : Attribute
{
    public object Id;

    public IdAttribute(object id)
    {
        Id = id;
    }
}