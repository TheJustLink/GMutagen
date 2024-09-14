using System;

namespace GMutagen.v8.Test;

public class ContractResolverFromContainer : IContractResolverChain
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