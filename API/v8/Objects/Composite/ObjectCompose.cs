using System;

using GMutagen.v8.Objects.Interface;

namespace GMutagen.v8.Objects.Composite;

public class ObjectCompose : IObject
{
    private readonly Object[] _objects;

    public ObjectCompose(Object[] objects)
    {
        _objects = objects;
    }

    public T Get<T>()
    {
        foreach (var state in _objects)
        {
            if (state.TryGet<T>(out var contract))
                return contract;
        }

        throw new Exception();
    }

    public bool TryGet<T>(out T contract)
    {
        foreach (var state in _objects)
        {
            if (state.TryGet(out contract))
                return true;
        }

        // ReSharper disable once NullableWarningSuppressionIsUsed
        contract = default!;
        return false;
    }
}