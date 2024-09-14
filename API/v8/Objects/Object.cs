using System;
using System.Collections.Generic;

using GMutagen.v8.IO;
using GMutagen.v8.Objects.Interface;
using GMutagen.v8.Objects.Stubs;
using GMutagen.v8.Objects.Template;

namespace GMutagen.v8.Objects;

public interface IGenerator<out T>
{
    T Generate();
}

public interface IGenerator<out TOut, in TIn> : IRead<TIn, TOut>
{
    TOut Generate(TIn input);
}

public class Object : IObject
{
    private readonly Dictionary<Type, ContractStub> _staticContracts;

    private readonly ObjectTemplate _template;

    public Object(Dictionary<Type, ContractStub> staticContracts, ObjectTemplate template)
    {
        _staticContracts = staticContracts;
        _template = template;
    }

    public T Get<T>()
    {
        return _staticContracts[typeof(T)].Get<T>();
    }

    public bool TryGet<T>(out T contract)
    {
        contract = default!;

        var success = _staticContracts.TryGetValue(typeof(T), out var contractStub);

        if (!success)
            return false;

        if (contractStub!.TryGet(out contract))
            return true;

        return false;
    }
}