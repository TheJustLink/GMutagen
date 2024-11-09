using System;
using System.Collections.Generic;

namespace GMutagen.v9.Contracts.Resolving.Nodes;

public class Context
{
    public readonly Context? ParentContext;
    public readonly Dictionary<object, object> Cache;
    
    public Type Type;
    public object? Instance;
    public object? Key;

    public Context(Type type, Context? parentContext = null, object? key = null)
    {
        Cache = new Dictionary<object, object>();
        Type = type;
        ParentContext = parentContext;
        Key = key;
    }
}