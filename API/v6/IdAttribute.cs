using System;

namespace GMutagen.v6;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class IdAttribute : Attribute
{
    public int Id;

    public IdAttribute(int id)
    {
        Id = id;
    }
}