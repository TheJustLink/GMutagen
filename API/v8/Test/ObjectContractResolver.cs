using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GMutagen.v8.Test;

public class ObjectContractResolver : IContractResolver
{
    private readonly IContractResolverChain _resolverChain;
    private readonly IServiceCollection _buildServices;

    public ObjectContractResolver(IContractResolverChain resolverChain, IServiceCollection buildServices)
    {
        _resolverChain = resolverChain;
        _buildServices = buildServices;
    }

    public object Resolve<TId>(ContractDescriptor contract, TId id) where TId : notnull
    {
        var buildServices = new ServiceCollection { _buildServices };
        buildServices.AddSingleton(new ObjectId(typeof(TId), id));

        var context = new ContractResolverContext(contract, buildServices);
        
        if (_resolverChain.Resolve(context) && context.Result is not null)
            return context.Result;

        throw new InvalidOperationException($"Can't resolve {context.Contract.Type}");
    }
}