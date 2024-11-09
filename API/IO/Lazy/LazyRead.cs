using System;

namespace GMutagen.IO.Lazy;

public class LazyRead<TId, TValue> : IRead<TId, TValue> where TId : notnull
{
    private readonly IRead<TId, TValue> _reader;
    private readonly IWrite<TId, TValue> _writer;
    private readonly Func<TValue> _valueFactory;

    public LazyRead(IReadWrite<TId, TValue> readWrite, Func<TValue> valueFactory)
        : this(readWrite, readWrite, valueFactory) { }
    public LazyRead(IRead<TId, TValue> reader, IWrite<TId, TValue> writer, Func<TValue> valueFactory)
    {
        _reader = reader;
        _writer = writer;
        _valueFactory = valueFactory;
    }

    public int Count => _reader.Count;
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