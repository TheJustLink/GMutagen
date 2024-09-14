using System;

namespace GMutagen.v8.IO;

public interface IRead<in TId, out TValue>
{
    TValue this[TId id] { get; }
    TValue Read(TId id);
    bool Contains(TId id);
}
public interface IRead<in TId>
{
    TValue Read<TValue>(TId id);
    bool Contains<TValue>(TId id);
}
public class LazyRead<TId, TValue> : IRead<TId, TValue>
{
    private readonly IRead<TId, TValue> _reader;
    private readonly IWrite<TId, TValue> _writer;
    private readonly Func<TValue> _valueFactory;

    public LazyRead(IRead<TId, TValue> reader, IWrite<TId, TValue> writer, Func<TValue> valueFactory)
    {
        _reader = reader;
        _writer = writer;
        _valueFactory = valueFactory;
    }

    public TValue this[TId id] => Read(id);
    public TValue Read(TId id)
    {
        if (_reader.Contains(id))
            return _reader.Read(id);

        var value = _valueFactory();
        _writer.Write(id, value);

        return value;
    }
    public bool Contains(TId id) => _reader.Contains(id);
}