using System;
using System.Collections.Generic;

using GMutagen.v8.Contracts;
using GMutagen.v8.Objects.Templates;

namespace GMutagen.v8.Objects;

public class ObjectBuilder<TId>
{
    private IObjectFactory<TId> _objectFactory;
    private readonly Dictionary<Type, ContractDescriptor> _contracts = new();

    public ObjectBuilder(IObjectFactory<TId> objectFactory, ObjectTemplate template)
        : this(objectFactory) => Add(template);
    public ObjectBuilder(IObjectFactory<TId> objectFactory)
    {
        _objectFactory = objectFactory;
    }

    public IObject<TId> Build()
    {
        return _objectFactory.Create(_contracts);
    }
    public IObject<TId> Build(TId id)
    {
        return _objectFactory.Create(_contracts, id);
    }

    public ObjectBuilder<TId> SetObjectFactory(IObjectFactory<TId> objectFactory)
    {
        _objectFactory = objectFactory;
        return this;
    }

    public ObjectBuilder<TId> Add(ObjectTemplate template)
    {
        foreach (var contract in template.Contracts)
            _contracts.Add(contract.Type, contract);

        return this;
    }
    public ObjectBuilder<TId> OverrideWith(ObjectTemplate template)
    {
        foreach (var contract in template.Contracts)
            _contracts[contract.Type] = contract;

        return this;
    }

    public ObjectBuilder<TId> Set<TContract, TImplementation>() where TContract : class
    {
        return Set(ContractDescriptor.Create<TContract, TImplementation>());
    }
    public ObjectBuilder<TId> Set<TContract>(TContract implementation) where TContract : class
    {
        return Set(ContractDescriptor.Create<TContract>(implementation));
    }

    private ObjectBuilder<TId> Set(ContractDescriptor contract)
    {
        if (_contracts.ContainsKey(contract.Type) is false)
            throw new ArgumentOutOfRangeException(nameof(contract.Type));

        _contracts[contract.Type] = contract;
        return this;
    }
}