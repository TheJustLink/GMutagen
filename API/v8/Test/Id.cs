using System;

namespace GMutagen.v8.Test;

public class Id
{
    public readonly Type Type;
    public readonly object Value;

    public Id(Type type, object value)
    {
        Type = type;
        Value = value;
    }
}