using System;
using System.Collections.Generic;

using GMutagen.Contracts.Resolving.Nodes;
using GMutagen.Extensions;
using GMutagen.Generators;
using GMutagen.Objects;

namespace GMutagen.Contracts.Resolving;

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

        return Create(contracts, objectId);
    }
    public IObject<TObjectId> Create(Dictionary<Type, ContractDescriptor> contracts, TObjectId objectId)
    {
        var objects = _services.GetObjectValues<TObjectId, TContractId>();

        var hasObject = objects.TryGet(objectId, out var objectValue);
        if (hasObject is false)
        {
            objectValue = objects[objectId] = new();
            SetContractDescriptorsFromObject(objectValue, contracts);
        }
        _buildServices.Set(objectValue);

        var implementations = CreateImplementations(contracts);

        return new Object<TObjectId>(objectId, implementations);
    }
    private void SetContractDescriptorsFromObject(ObjectValue<TContractId> objectValue, Dictionary<Type, ContractDescriptor> contractDescriptors)
    {
        var contracts = _services.GetContractValues<TContractId, TSlotId, TValueId>();

        foreach (var contract in objectValue)
        {
            var contractValue = contracts.Read(contract.Value);
            contractDescriptors[contract.Key] = new ContractDescriptor(contractValue.Type);
        }
    }
    private Dictionary<Type, object> CreateImplementations(Dictionary<Type, ContractDescriptor> contracts)
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