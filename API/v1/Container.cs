using System;
using System.Collections.Generic;

namespace GMutagen.v1;

public class Container
{
    private Dictionary<Type, object> _contractMap;

    public Container()
    {
        _contractMap = new Dictionary<Type, object>();
    }

    public Container(Container container)
    {
        _contractMap = new Dictionary<Type, object>();

        foreach (var pair in container._contractMap)
            _contractMap.Add(pair.Key, pair.Value);
    }

    public T Get<T>() where T : class
    {
        var obj = _contractMap[typeof(T)];

        if (obj is EmptyContract)
            throw new InvalidOperationException("Contract was not set yet");

        return (T)obj;
    }
    public bool TryGet<T>(out T contract) where T : class
    {
        var isSuccess = _contractMap.TryGetValue(typeof(T), out var result);

        contract = (result as T)!;

        return isSuccess;
    }

    public void Set<T>(T contract) where T : class
    {
        var type = contract.GetType();
        _contractMap[type] = contract;

        foreach (var inter in type.GetInterfaces())
            _contractMap[inter] = contract;
    }

    public void AddEmpty<T>() where T : class
    {
        _contractMap.Add(typeof(T), new EmptyContract());
    }
    public void Add<T>() where T : class, new()
    {
        Add(new T());
    }
    public void Add<T>(T contract) where T : class
    {
        _contractMap.Add(typeof(T), contract);
    }

    public Container Clone()
    {
        return new Container(this);
    }
}