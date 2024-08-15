using System;
using System.Collections.Generic;
using System.Numerics;

namespace GMutagen.v5;

class Test
{
    public static void Main()
    {
        Game(new GuidGenerator());
        Game(new IncrementalGenerator<int>());
    }

    private static void Game<TId>(IGenerator<TId> idGenerator)
    {
        var positionGenerator = GetDefaultPositionGenerator(idGenerator);

        var playerTemplate = new ObjectTemplate();
        playerTemplate.AddEmpty<IPosition>();

        var player = playerTemplate.Create();
        player.Set<IPosition>(positionGenerator.Generate());
    }
    public static IGenerator<IPosition> GetDefaultPositionGenerator<TId>(IGenerator<TId> idGenerator)
    {
        IReadWrite<TId, Vector2> positions = new MemoryRepository<TId, Vector2>();
        IGenerator<IValue<Vector2>> positionValueGenerator = new ExternalValueGenerator<TId, Vector2>(idGenerator, positions);
        IGenerator<IValue<Vector2>> lazyPositionValueGenerator = new GeneratorDecorator<IValue<Vector2>>(positionValueGenerator, new LazyValueGenerator<Vector2>());

        ITypeReadWrite<IGenerator<object>> valueGenerators = new TypeRepository<IGenerator<object>>(); // fix it pls later pls pls pls
        valueGenerators.Write(lazyPositionValueGenerator);

        ITypeRead<IGenerator<object>> universalGenerator = valueGenerators;

        var positionGenerator = CreateContractGenerator<IPosition>(universalGenerator, new DefaultPositionGenerator());
        return positionGenerator;
    }
    public static IGenerator<TContract> CreateContractGenerator<TContract>(ITypeRead<IGenerator<object>> universalGenerator, IGenerator<TContract, ITypeRead<IGenerator<object>>> contractGenerator)
    {

        // return new GeneratorCache<TContract>(universalGenerator, contractGenerator);
    }
}

public class DefaultPosition : IPosition
{
    private readonly IValue<Vector2> _currentPosition;
    private readonly IValue<Vector2> _previousPosition;

    public DefaultPosition()
    {
    }

    public DefaultPosition(IValue<Vector2> currentPosition, IValue<Vector2> previousPosition)
    {
        _currentPosition = currentPosition;
        _previousPosition = previousPosition;
    }

    public Vector2 GetCurrentPositionWithOffset(Vector2 offset)
    {
        return _currentPosition.Value + offset;
    }
    public Vector2 GetPreviousPositionWithOffset(Vector2 offset)
    {
        return _previousPosition.Value + offset;
    }
}
public interface IPosition
{
    Vector2 GetCurrentPositionWithOffset(Vector2 offset);
    Vector2 GetPreviousPositionWithOffset(Vector2 offset);
}
public class ValueWithHistory<T> : IValue<T>
{
    private readonly IValue<T> _source;
    private T _previousValue;

    public ValueWithHistory(IValue<T> source, bool preInit = false)
    {
        _source = source;

        if (preInit)
            _previousValue = source.Value;
    }

    public T Value
    {
        get => _previousValue;
        set
        {
            _previousValue = _source.Value;
            _source.Value = value;
        }
    }
}
public class DefaultPositionGenerator : IGenerator<IPosition, ITypeRead<IGenerator<object>>>
{
    public IPosition Generate(ITypeRead<IGenerator<object>> input)
    {
        var currentPosition = input.Read<IGenerator<IValue<Vector2>>>().Generate();
        var previousPosition = new ValueWithHistory<Vector2>(currentPosition);

        return new DefaultPosition(currentPosition, previousPosition);
    }

    public IPosition this[ITypeRead<IGenerator<object>> id] => Read(id);
    public IPosition Read(ITypeRead<IGenerator<object>> id) => Generate(id);
}

public class LazyValueGenerator<T> : IGenerator<LazyValue<T>, IGenerator<IValue<T>>>
{
    public LazyValue<T> Generate(IGenerator<IValue<T>> generator) => new(generator);

    public LazyValue<T> this[IGenerator<IValue<T>> id] => Read(id);
    public LazyValue<T> Read(IGenerator<IValue<T>> id) => Generate(id);
}

public class GeneratorDecorator<T> : GeneratorDecorator<T, T>
{
    public GeneratorDecorator(IGenerator<T> sourceGenerator, IGenerator<T, IGenerator<T>> proxyGenerator)
        : base(sourceGenerator, proxyGenerator) { }
}
public class GeneratorDecorator<TIn, TOut> : IGenerator<TOut>
{
    private readonly IGenerator<TIn> _sourceGenerator;
    private readonly IGenerator<TOut, IGenerator<TIn>> _proxyGenerator;

    public GeneratorDecorator(IGenerator<TIn> sourceGenerator, IGenerator<TOut, IGenerator<TIn>> proxyGenerator)
    {
        _sourceGenerator = sourceGenerator;
        _proxyGenerator = proxyGenerator;
    }

    public TOut Generate() => _proxyGenerator.Generate(_sourceGenerator);
}

public class LazyValue<T> : IValue<T>
{
    private readonly IGenerator<IValue<T>> _valueGenerator;
    private IValue<T>? _cachedValue;

    public LazyValue(IGenerator<IValue<T>> valueGenerator)
    {
        _valueGenerator = valueGenerator;
    }

    public T Value
    {
        get => (_cachedValue ??= _valueGenerator.Generate()).Value;
        set => (_cachedValue ??= _valueGenerator.Generate()).Value = value;
    }
}

public class ExternalValueGenerator<TId, TValue> : IGenerator<IValue<TValue>>
{
    private readonly IGenerator<TId> _idGenerator;
    private readonly IReadWrite<TId, TValue> _readWrite;

    public ExternalValueGenerator(IGenerator<TId> idGenerator, IReadWrite<TId, TValue> readWrite)
    {
        _idGenerator = idGenerator;
        _readWrite = readWrite;
    }

    public IValue<TValue> Generate()
    {
        var id = _idGenerator.Generate();

        return new ExternalValue<TId, TValue>(id, _readWrite);
    }
}
public class GuidGenerator : IGenerator<Guid>
{
    public Guid Generate() => Guid.NewGuid();
}

public class IncrementalGenerator<T> : IValue<T>, IGenerator<T> where T : IAdditionOperators<T, int, T>
{
    public T Value { get; set; }

    public T Generate()
    {
        var id = Value;

        Value += 1;

        return id;
    }
}
public interface IGenerator<out TOut, in TIn> : IRead<TIn, TOut>
{
    TOut Generate(TIn input);
}
public interface IGenerator<out T>
{
    T Generate();
}
public class TypeRepository<TValue> : MemoryRepository<Type, TValue>, ITypeReadWrite<TValue> where TValue : class
{
    public T Read<T>() where T : class
    {
        return Read(typeof(T)) as T;
    }

    public void Write<T>(T value)
    {
        Write(typeof(T), value as TValue);
    }
}

public class MemoryRepository<TId, TValue> : IReadWrite<TId, TValue> where TId : notnull
{
    private readonly Dictionary<TId, TValue> _memory = new();

    public TValue this[TId id]
    {
        get => Read(id);
        set => Write(id, value);
    }

    public TValue Read(TId id)
    {
        return _memory[id];
    }
    public void Write(TId id, TValue value)
    {
        _memory[id] = value;
    }
}

public interface ITypeReadWrite<out TValue> : ITypeRead<TValue>, ITypeWrite<TValue>
{
}

public interface ITypeWrite<out TValue> : IRead<Type, TValue>
{
    void Write<T>(T value);
}

public interface ITypeRead<out TValue> : IRead<Type, TValue>
{
    T Read<T>() where T : class;
}

public interface IReadWrite<in TId, TValue> : IRead<TId, TValue>, IWrite<TId, TValue>
{
    TValue this[TId id] { get; set; }
}

public interface IRead<in TId, out TValue>
{
    TValue this[TId id] { get; }
    TValue Read(TId id);
}

public interface IWrite<in TId, in TValue>
{
    TValue this[TId id] { set; }
    void Write(TId id, TValue value);
}

public static class ReadWriteExtensions
{
    public static ExternalValue<TId, TValue> Instantiate<TId, TValue>(this IReadWrite<TId, TValue> readWrite,
        IGenerator<TId> idGenerator)
    {
        return new ExternalValue<TId, TValue>(idGenerator.Generate(), readWrite);
    }

    public static ExternalValue<TId, TValue> GetInstance<TId, TValue>(this IReadWrite<TId, TValue> readWrite, TId id)
    {
        return new ExternalValue<TId, TValue>(id, readWrite);
    }
}

public class ExternalValue<TId, TValue> : IValue<TValue>
{
    private readonly TId _id;
    private readonly IRead<TId, TValue> _reader;
    private readonly IWrite<TId, TValue> _writer;

    public ExternalValue(TId id, IReadWrite<TId, TValue> readWrite)
    {
        _id = id;
        _reader = readWrite;
        _writer = readWrite;
    }

    public ExternalValue(TId id, IRead<TId, TValue> reader, IWrite<TId, TValue> writer)
    {
        _id = id;
        _reader = reader;
        _writer = writer;
    }

    public TValue Value
    {
        get => _reader.Read(_id);
        set => _writer.Write(_id, value);
    }
}

public interface IValue<T> : IValue
{
    T Value { get; set; }
}

public interface IValue
{
}

public class Object
{
    private readonly Dictionary<Type, object> _contracts;

    public Object(Dictionary<Type, object> contracts)
    {
        _contracts = contracts;
    }

    public T Get<T>()
    {
        return (T)_contracts[typeof(T)];
    }
}

internal interface IFromReflection
{
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class IdAttribute : Attribute
{
    public int Id;

    public IdAttribute(int id)
    {
        Id = id;
    }
}

public class EmptyContract
{
}