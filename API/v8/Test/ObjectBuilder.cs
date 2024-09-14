using System;
using System.Collections.Generic;

namespace GMutagen.v8.Test;

public class ObjectBuilder
{
    private IObjectFactory _objectFactory;
    private readonly Dictionary<Type, ContractDescriptor> _contracts = new();

    public ObjectBuilder(IObjectFactory objectFactory, ObjectTemplate template)
        : this(objectFactory) => Add(template);
    public ObjectBuilder(IObjectFactory objectFactory)
    {
        _objectFactory = objectFactory;
    }

    public IObject Build()
    {
        return _objectFactory.Create(_contracts);
    }

    public ObjectBuilder SetObjectFactory(IObjectFactory objectFactory)
    {
        _objectFactory = objectFactory;
        return this;
    }

    public ObjectBuilder Add(ObjectTemplate template)
    {
        foreach (var contract in template.Contracts)
            _contracts.Add(contract.Type, contract);

        return this;
    }
    public ObjectBuilder OverrideWith(ObjectTemplate template)
    {
        foreach (var contract in template.Contracts)
            _contracts[contract.Type] = contract;

        return this;
    }

    public ObjectBuilder Set<TContract, TImplementation>() where TContract : class
    {
        return Set(ContractDescriptor.Create<TContract, TImplementation>());
    }
    public ObjectBuilder Set<TContract>(TContract implementation) where TContract : class
    {
        return Set(ContractDescriptor.Create<TContract>(implementation));
    }

    private ObjectBuilder Set(ContractDescriptor contract)
    {
        if (_contracts.ContainsKey(contract.Type) is false)
            throw new ArgumentOutOfRangeException(nameof(contract.Type));

        _contracts[contract.Type] = contract;
        return this;
    }
}