using System;
using System.Collections.Generic;

namespace GMutagen.v8.Objects;

public class Object<TId> : IObject
{
    private readonly TId _id;
    private readonly Dictionary<Type, object> _contracts;

    public Object(TId id, Dictionary<Type, object> contracts)
    {
        _id = id;
        _contracts = contracts;
    }

    public TContract Get<TContract>() where TContract : class
    {
        return (TContract)_contracts[typeof(TContract)];
    }
}