using GMutagen.v8.IO;

namespace GMutagen.v8.Values;

public class ExternalValue<TId, TValue> : IValue<TValue>
{
    private readonly TId _id;
    private readonly IRead<TId, TValue> _reader;
    private readonly IWrite<TId, TValue> _writer;

    public ExternalValue(TId id, IReadWrite<TId, TValue> readWrite) : this(id, readWrite, readWrite)
    {

    }

    public ExternalValue(TId id, IRead<TId, TValue> reader, IWrite<TId, TValue> writer)
    {
        _id = id;
        _reader = reader;
        _writer = writer;
        _writer.Write(id, default);
    }

    public TValue Value
    {
        get => _reader.Read(_id);
        set => _writer.Write(_id, value);
    }
}