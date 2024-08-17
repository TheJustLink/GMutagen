using System;
using System.Collections.Generic;
using GMutagen.v5.Container;

namespace GMutagen.v5;

public class ObjectTemplate
{
    private readonly ObjectTemplateContainer _container;
    private readonly Dictionary<Type, object> _contracts;

    public ObjectTemplate()
    {
        _contracts = new Dictionary<Type, object>();
        _container = new ObjectTemplateContainer();
    }

    public Object Create()
    {
        var instanceContracts = new Dictionary<Type, object>();

        foreach (var pair in _contracts)
        {
            var instance = _container.Resolve(pair.Key);
            instanceContracts.Add(pair.Key, instance);
        }

        return new Object(instanceContracts);
    }

    public ObjectTemplate AddEmpty<T>()
    {
        _contracts.Add(typeof(T), new EmptyContract());
        _container.Add<T>();
        return this;
    }

    public ObjectTemplate Add<T>(T value)
    {
        _contracts.Add(typeof(T), value);
        _container.Add<T>().FromInstance(value);
        return this;
    }

    public ObjectTemplate Set<T>(T value)
    {
        _contracts[typeof(T)] = value;
        _container.Add<T>().FromInstance(value);
        return this;
    }
}