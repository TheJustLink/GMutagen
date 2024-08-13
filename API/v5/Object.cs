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
        IReadWrite<TId, Vector2> positions = new MemoryValues<TId, Vector2>();
        var positionGenerator = new ExternalValueGenerator<TId, Vector2>(idGenerator, positions);

        var playerTemplate = new ObjectTemplate();
        playerTemplate.AddEmpty<IPosition>();

        var player = playerTemplate.Create();

        var playerPosition = new LazyValue<Vector2>(positionGenerator);
        var playerPrevPosition = positionGenerator.Generate();

        player.Set<IPosition>(new DefaultPosition(playerPosition, playerPrevPosition));
    }
}

public class DefaultPosition : IPosition
{
    private readonly IValue<Vector2> _currentPosition;
    private readonly IValue<Vector2> _previousPosition;

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
public interface IGenerator<out T>
{
    T Generate();
}

public class MemoryValues<TId, TValue> : IReadWrite<TId, TValue> where TId : notnull
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

public interface IReadWrite<in TId, TValue> : IRead<TId, TValue>, IWrite<TId, TValue> { }
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
    public static ExternalValue<TId, TValue> Instantiate<TId, TValue>(this IReadWrite<TId, TValue> readWrite, IGenerator<TId> idGenerator)
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
public interface IValue<T>
{
    T Value { get; set; }
}


public class Object
{
    private Dictionary<Type, object> _contracts;

    public Object(Dictionary<Type, object> contracts)
    {
        _contracts = contracts;
    }

    public T Get<T>()
    {
        return (T)_contracts[typeof(T)];
    }
    public void Set<T>(T value)
    {
        _contracts[typeof(T)] = value;
    }
}
public class ObjectTemplate
{
    private readonly Dictionary<Type, object> _contracts = new();

    public Object Create()
    {
        return new Object(_contracts);
    }

    public void AddEmpty<T>()
    {
        _contracts.Add(typeof(T), new EmptyContract());
    }
    public void Add<T>(T value)
    {
        _contracts.Add(typeof(T), value);
    }

    public void Set<T>(T value)
    {
        _contracts[typeof(T)] = value;
    }
}

public class EmptyContract { }