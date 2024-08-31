using System;
using System.Collections.Generic;

namespace GMutagen.v6;

public class GeneratorDecorator<TIn, TOut> : IGenerator<TOut>
{
    private readonly IGenerator<TIn> _sourceGenerator;
    private readonly IGenerator<TOut, IGenerator<TIn>> _proxyGenerator;

    protected GeneratorDecorator(IGenerator<TIn> sourceGenerator, IGenerator<TOut, IGenerator<TIn>> proxyGenerator)
    {
        _sourceGenerator = sourceGenerator;
        _proxyGenerator = proxyGenerator;
    }

    public TOut Generate() => _proxyGenerator.Generate(_sourceGenerator);
}

public interface IGenerator<out T>
{
    T Generate();
}

public class Object : IObject
{
    private readonly Dictionary<Type, ContractStub> _staticContracts;

    // ReSharper disable once NotAccessedField.Local
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
        // ReSharper disable once NullableWarningSuppressionIsUsed
        contract = default!;

        var success = _staticContracts.TryGetValue(typeof(T), out var contractStub);

        if (!success)
            return false;

        // ReSharper disable once NullableWarningSuppressionIsUsed
        if (contractStub!.TryGet(out contract))
            return true;

        return false;
    }
}