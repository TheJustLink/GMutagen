using System;
using GMutagen.v9.Extensions;

namespace GMutagen.v9.Contracts.Resolving.Nodes.From;

public class FromContainer : IContractResolverNode
{
    private readonly IServiceProvider _services;
    
    public FromContainer(IServiceProvider services)
    {
        _services = services;
    }

    public bool Resolve(Context context)
    {
        context.Instance = context.Key is not null
            ? _services.GetKeyedService(context.Type, context.Key)
            : _services.GetService(context.Type);

        return context.Instance is not null;
    }
}