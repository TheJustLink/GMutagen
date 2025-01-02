using System;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Extensions;

namespace GMutagen.v9.Contracts.Resolving.Nodes.From;

public class FromContainer(IServiceProvider services) : IResolverNode
{
    public bool Resolve(Context context)
    {
        var success = context.TryGetKey<Type>(KeyType.DeclaredType, out var type);
        if(success is false)
            return false;
        
        if (context.Keys == null)
        {
            var instance = services.GetService(type);
            context.Instance = instance;
            return instance is not null;
        }

        foreach (var key in context.Keys)
        {
            var instance = services.GetKeyedService(type, key);
            context.Instance = instance;
            
            if(instance is not null)
                return true;
        }

        return false;
    }
}