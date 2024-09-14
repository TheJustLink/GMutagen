using System;
using System.Collections.Generic;

using GMutagen.v8.Objects;

namespace GMutagen.v8.Test;

public class DefaultObjectFactory<TId> : IObjectFactory where TId : notnull
{
    private readonly IGenerator<TId> _idGenerator;
    private readonly IContractResolver _contractResolver;

    public DefaultObjectFactory(IGenerator<TId> idGenerator, IContractResolver contractResolver)
    {
        _idGenerator = idGenerator;
        _contractResolver = contractResolver;
    }

    public IObject Create(Dictionary<Type, ContractDescriptor> contracts)
    {
        var id = _idGenerator.Generate();
        Console.WriteLine("CREATING OBJECT " + id);
        var implementations = new Dictionary<Type, object>(contracts.Count);

        foreach (var contract in contracts.Values)
            implementations[contract.Type] = _contractResolver.Resolve(contract, id);

        return new Object<TId>(id, implementations);
    }
}