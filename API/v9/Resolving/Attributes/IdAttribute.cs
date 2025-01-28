using System;

namespace GMutagen.v9.Resolving.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class IdAttribute : Attribute
{
    public object Id;

    public IdAttribute(object id)
    {
        Id = id;
    }
}