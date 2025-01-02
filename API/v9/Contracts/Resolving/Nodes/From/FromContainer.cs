using System;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Extensions;

namespace GMutagen.v9.Contracts.Resolving.Nodes.From;

public class FromContainer : IResolverNode
{
    private readonly IServiceProvider _services;
    
    public FromContainer(IServiceProvider services)
    {
        _services = services;
    }

    public bool Resolve(Context context)
    {
        if (context.Keys == null)
        {
            var instance = _services.GetService(context.Type);
            context.Instance = instance;
            return instance is not null;
        }

        foreach (var key in context.Keys)
        {
            var instance = _services.GetKeyedService(context.Type, key);
            context.Instance = instance;
            
            if(instance is not null)
                return true;
        }

        return false;
    }
}