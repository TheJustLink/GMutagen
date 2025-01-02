using System;
using System.Collections.Generic;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Contracts.Resolving.Contexts.Option;
using GMutagen.v9.Contracts.Resolving.Nodes;
using GMutagen.v9.Generators;
using GMutagen.v9.Objects;

namespace GMutagen.v9.Contracts.Resolving;

public class ResolvingObjectFactory<TObjectId>(
    IGenerator<TObjectId> idGenerator,
    IResolverNode resolver) : IObjectFactory<TObjectId>
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

        var options = new Options()
            .Add(OptionType.Contracts, contracts);
        
        var objContext = new Context(typeof(Object<TObjectId>), keys, options);
        
        if(resolver.Resolve(objContext) is false || objContext.Instance == null)
            throw new InvalidOperationException($"Can't resolve {objContext.Type}");

        var obj = objContext.Instance;
        return (IObject<TObjectId>)obj;
    }
    
}