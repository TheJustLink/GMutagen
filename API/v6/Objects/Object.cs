using System;
using System.Collections.Generic;
using GMutagen.v6.IO;
using GMutagen.v6.Objects.Interface;
using GMutagen.v6.Objects.Stubs;
using GMutagen.v6.Objects.Template;

namespace GMutagen.v6.Objects;

public interface IGenerator<out T>
{
    T Generate();
}

public interface IGenerator<out TOut, in TIn> : IRead<TIn, TOut>
{
    TOut Generate(TIn input);
}

public abstract class GeneratorDecorator<T> : IGenerator<T>
{
    protected IGenerator<T> Child;

    public GeneratorDecorator(IGenerator<T> child)
    {
        Child = child;
    }

    public virtual T Generate()
    {
        return Child.Generate();
    }
}

public class GeneratorAdapter<TResult, TId> : IGenerator<TResult, TId>
{
    protected readonly IGenerator<TResult> Child;

    public GeneratorAdapter(IGenerator<TResult> child)
    {
        Child = child;
    }

    public TResult this[TId id] => Read(id);

    public virtual TResult Read(TId id)
    {
        return this[id];
    }

    public virtual TResult Generate(TId input)
    {
        return Child.Generate();
    }
}

public class GeneratorAdapter2<TResult, TId> : IGenerator<TResult>
{
    protected readonly IGenerator<TId> IdGenerator;
    protected readonly IGenerator<TResult, TId> Child;

    public GeneratorAdapter2(IGenerator<TResult, TId> child, IGenerator<TId> idGenerator)
    {
        Child = child;
        IdGenerator = idGenerator;
    }

    public TResult this[TId id] => Read(id);

    public virtual TResult Read(TId id)
    {
        return this[id];
    }

    public virtual TResult Generate()
    {
        var id = IdGenerator.Generate();
        return Child.Generate(id);
    }
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