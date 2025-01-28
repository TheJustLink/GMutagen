using System;
using System.Collections.Generic;
using GMutagen.v9.Contracts.Descriptors;
using GMutagen.v9.Generators.Interfaces;
using GMutagen.v9.Logging.Logger.Common;
using GMutagen.v9.Logging.Logger.Interfaces;
using GMutagen.v9.Logging.Messages.Realizations;
using GMutagen.v9.Objects.Factories.Interfaces;
using GMutagen.v9.Objects.Interfaces;
using GMutagen.v9.Resolving.Contexts;
using GMutagen.v9.Resolving.Contexts.Key;
using GMutagen.v9.Resolving.Contexts.Option;
using GMutagen.v9.Resolving.Nodes.Interfaces;

namespace GMutagen.v9.Objects.Factories;

public class ResolvingObjectFactory<TObjectId>(
    IGenerator<TObjectId> idGenerator,
    IResolverNode resolver, ILogger<Global> logger) : IObjectFactory<TObjectId>
    where TObjectId : notnull
{
    public IObject<TObjectId> Create(Dictionary<Type, ContractDescriptor> contracts)
    {
        var objectId = idGenerator.Generate();

        return Create(contracts, objectId);
    }

    public IObject<TObjectId> Create(Dictionary<Type, ContractDescriptor> contracts, TObjectId objectId)
    {
        var keys = new Keys()
            .Add(KeyType.Id, objectId);

        var reversedContracts = new Dictionary<Type, List<Type>>();

        foreach (var (type, descriptor) in contracts)
        {
            if (reversedContracts.TryGetValue(descriptor.ImplementationType, out var list))
                list.Add(type);
            else
                reversedContracts[descriptor.ImplementationType] = new List<Type>() { type };
        }

        var options = new Options()
            .Add(OptionType.Contracts, contracts)
            .Add(OptionType.ContractsReversed, reversedContracts);

        var objContext = new Context(typeof(Object<TObjectId>), keys, options);

        if (resolver.Resolve(objContext) is false || objContext.Instance == null)
        {
            logger.LogError(new FailedToResolve(objContext, this));
            throw new InvalidOperationException($"Can't resolve {objContext.Type}");
        }

        var obj = objContext.Instance;
        logger.LogInfo(new SuccessfullyResolved(objContext, this));
        return (IObject<TObjectId>)obj;
    }
}