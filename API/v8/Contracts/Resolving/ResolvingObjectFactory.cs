using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

using GMutagen.v8.Contracts.Resolving.Nodes;
using GMutagen.v8.Extensions;
using GMutagen.v8.Generators;
using GMutagen.v8.Objects;

namespace GMutagen.v8.Contracts.Resolving;

public class ResolvingObjectFactory<TObjectId, TContractId> : IObjectFactory<TObjectId> where TObjectId : notnull
{
    private readonly IServiceProvider _services;
    private readonly IGenerator<TObjectId> _idGenerator;
    private readonly IContractResolverNode _contractResolverNode;

    public ResolvingObjectFactory(IServiceProvider services, IGenerator<TObjectId> idGenerator, IContractResolverNode contractResolverNode)
    {
        _services = services;
        _idGenerator = idGenerator;
        _contractResolverNode = contractResolverNode;
    }

    public IObject<TObjectId> Create(Dictionary<Type, ContractDescriptor> contracts)
    {
        var objectId = _idGenerator.Generate();
        var objects = _services.GetObjectValues<TObjectId, TContractId>();

        if (objects.TryGet(objectId, out var objectValue) is false)
            objectValue = objects[objectId] = new();

        var buildServices = new ServiceCollection()
            .AddSingleton(objectValue);

        var implementations = new Dictionary<Type, object>(contracts.Count);

        foreach (var contract in contracts.Values)
            implementations[contract.Type] = Resolve(contract, buildServices);

        return new Object<TObjectId>(objectId, implementations);
    }

    private object Resolve(ContractDescriptor contract, IServiceCollection buildServices)
    {
        var context = new ContractResolverContext(contract, buildServices);

        if (_contractResolverNode.Resolve(context) && context.Result is not null)
            return context.Result;

        throw new InvalidOperationException($"Can't resolve {context.Contract.Type}");
    }
}