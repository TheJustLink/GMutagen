using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using GMutagen.v8.Contracts.Resolving.BuildServices;
using GMutagen.v8.Contracts.Resolving.Nodes;
using GMutagen.v8.Generators;
using GMutagen.v8.Objects;

namespace GMutagen.v8.Contracts.Resolving;

public class ResolvingObjectFactory<TId> : IObjectFactory<TId> where TId : notnull
{
    private readonly IGenerator<TId> _idGenerator;
    private readonly IContractResolverNode _contractResolverNode;

    public ResolvingObjectFactory(IGenerator<TId> idGenerator, IContractResolverNode contractResolverNode)
    {
        _idGenerator = idGenerator;
        _contractResolverNode = contractResolverNode;
    }

    public IObject<TId> Create(Dictionary<Type, ContractDescriptor> contracts)
    {
        var id = _idGenerator.Generate();
        var buildServices = new ServiceCollection()
            .AddSingleton(new ObjectId(typeof(TId), id));

        var implementations = new Dictionary<Type, object>(contracts.Count);

        foreach (var contract in contracts.Values)
            implementations[contract.Type] = Resolve(contract, buildServices);

        return new Object<TId>(id, implementations);
    }

    private object Resolve(ContractDescriptor contract, IServiceCollection buildServices)
    {
        var context = new ContractResolverContext(contract, buildServices);

        if (_contractResolverNode.Resolve(context) && context.Result is not null)
            return context.Result;

        throw new InvalidOperationException($"Can't resolve {context.Contract.Type}");
    }
}