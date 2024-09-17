using System;

using GMutagen.v8.Contracts.Resolving.Attributes;
using GMutagen.v8.IO;

namespace Roguelike;

public class InLoggerAttribute : ValueLocationAttribute { }

public class LoggerRepository<TId, TValue> : IReadWrite<TId, TValue> where TId : notnull
{
    private readonly IReadWrite<TId, TValue> _source;

    public TValue this[TId id]
    {
        get => Read(id);
        set => Write(id, value);
    }

    public LoggerRepository(IReadWrite<TId, TValue> source)
    {
        _source = source;
    }

    public void Write(TId id, TValue value)
    {
        Console.WriteLine($"Write [{id}] <= [{value}]");
        _source.Write(id, value);
    }
    public TValue Read(TId id)
    {
        var value = _source.Read(id);
        Console.WriteLine($"Read [{id}] => [{value}]");

        return value;
    }
    public bool Contains(TId id)
    {
        var value = _source.Contains(id);
        Console.WriteLine($"Contains [{id}] => [{value}]");

        return value;
    }

    public static LoggerRepository<TId, TValue> Create(IReadWrite<TId, TValue> source)
    {
        return new LoggerRepository<TId, TValue>(source);
    }
}