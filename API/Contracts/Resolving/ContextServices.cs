using System;
using System.Collections.Generic;

namespace GMutagen.Contracts.Resolving;

public class ContextServices : Dictionary<Type, object>
{
    public T? Get<T>() where T : class => this[typeof(T)] as T;
    public void Set<T>(T contextService) where T : class => this[typeof(T)] = contextService!;

    public bool TryGet<T>(out T service) where T : class
    {
        var result = TryGetValue(typeof(T), out var value);
        service = (value as T)!;

        return result;
    }
}