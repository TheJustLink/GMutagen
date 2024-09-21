using System;
using System.Collections.Generic;

using GMutagen.v8.Contracts.Resolving.Nodes;
using GMutagen.v8.Extensions;
using GMutagen.v8.Generators;
using GMutagen.v8.Objects;

namespace GMutagen.v8.Contracts.Resolving;

public class ResolvingObjectFactory<TObjectId, TContractId, TSlotId, TValueId> : IObjectFactory<TObjectId>
    where TObjectId : notnull
    where TContractId : notnull
    where TSlotId : notnull
{
    private readonly IServiceProvider _services;
    private readonly IGenerator<TObjectId> _idGenerator;
    private readonly IContractResolverNode _contractResolverNode;
    private readonly ContextServices _buildServices;

    public ResolvingObjectFactory(IServiceProvider services, IGenerator<TObjectId> idGenerator, IContractResolverNode contractResolverNode)
    {
        _services = services;
        _idGenerator = idGenerator;
        _contractResolverNode = contractResolverNode;
        
        _buildServices = new();
        _buildServices.Set(new ContractRuntimeCache<TContractId>());
    }

    public IObject<TObjectId> Create(Dictionary<Type, ContractDescriptor> contracts)
    {
        var objectId = _idGenerator.Generate();
        var objects = _services.GetObjectValues<TObjectId, TContractId>();
        
        Dictionary<Type, object> implementations;

        var hasObject = objects.TryGet(objectId, out var objectValue);
        if (hasObject is false)
        {
            objectValue = objects[objectId] = new();
            _buildServices.Set(objectValue);
            implementations = CreateNewImplementations(contracts);
        }
        else
        {
            _buildServices.Set(objectValue);
            implementations = LoadImplementations(objectValue);
        }

        return new Object<TObjectId>(objectId, implementations);
    }
    private Dictionary<Type, object> LoadImplementations(ObjectValue<TContractId> objectValue)
    {
        var contracts = _services.GetContractValues<TContractId, TSlotId, TValueId>();
        var implementations = new Dictionary<Type, object>(objectValue.Count);

        foreach (var contract in objectValue)
        {
            var contractValue = contracts.Read(contract.Value);
            var contractDescriptor = new ContractDescriptor(contractValue.Type);
            var context = new ContractResolverContext(contractDescriptor, _buildServices)
            {
                Key = contract.Key
            };

            implementations[contract.Key] = Resolve(context);
        }

        return implementations;
    }
    private Dictionary<Type, object> CreateNewImplementations(Dictionary<Type, ContractDescriptor> contracts)
    {
        var implementations = new Dictionary<Type, object>(contracts.Count);

        foreach (var contract in contracts)
        {
            var context = new ContractResolverContext(contract.Value, _buildServices)
            {
                Key = contract.Key
            };

            implementations[contract.Key] = Resolve(context);
        }

        return implementations;
    }

    private object Resolve(ContractResolverContext context)
    {
        if (_contractResolverNode.Resolve(context) && context.Result is not null)
            return context.Result;

        throw new InvalidOperationException($"Can't resolve {context.Contract.Type}");
    }
}