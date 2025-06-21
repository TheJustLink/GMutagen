using System;
using System.Collections.Generic;
using GMutagen.v9.Objects.Interfaces;

namespace GMutagen.v9.Objects;

public class Object<TId> : IObject<TId>
{
    public TId Id { get; }

    private readonly Dictionary<Type, object> _contracts;

    public Object(TId id, Dictionary<Type, object> contracts)
    {
        Id = id;
        _contracts = contracts;
    }

    public TContract Get<TContract>() where TContract : class
    {
        return (TContract)_contracts[typeof(TContract)];
    }

    public bool TryGet<TContract>(out TContract contract) where TContract : class
    {
        var result = _contracts.TryGetValue(typeof(TContract), out var contractObj);
        if (!result)
        {
            contract = null!;
            return result;
        }

        contract = (TContract)contractObj!;
        return result;
    }
}