using System;
using GMutagen.v8.Extensions;

namespace GMutagen.v8.Contracts.Resolving.Nodes;

public class ContractResolverFromContainer : IContractResolverNode
{
    private readonly IServiceProvider _services;
    public ContractResolverFromContainer(IServiceProvider services)
    {
        _services = services;
    }

    public bool Resolve(ContractResolverContext context)
    {
        context.Result = context.Key is not null
            ? _services.GetKeyedService(context.Contract.Type, context.Key)
            : _services.GetService(context.Contract.Type);

        return context.Result is not null;
    }
}