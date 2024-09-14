namespace GMutagen.v8.IO;

public class ReadWriteTypeCasted<TId, TValue> : IReadWrite<TId, TValue>
{
    private readonly IRead<TId, object> _reader;
    private readonly IWrite<TId, object> _writer;

    public ReadWriteTypeCasted(IReadWrite<TId, object> readWrite)
        : this(readWrite, readWrite) { }
    public ReadWriteTypeCasted(IRead<TId, object> reader, IWrite<TId, object> writer)
    {
        _reader = reader;
        _writer = writer;
    }

    public TValue this[TId id]
    {
        get => (TValue)_reader[id];
        set => _writer[id] = value!;
    }

    public void Write(TId id, TValue value) => _writer.Write(id, value!);
    public TValue Read(TId id) => (TValue)_reader.Read(id);
    public bool Contains(TId id) => _reader.Contains(id);
}
public class ReadWrite<TId, TValue> : IReadWrite<TId, TValue>
{
    private readonly IRead<TId, TValue> _reader;
    private readonly IWrite<TId, TValue> _writer;

    public ReadWrite(IRead<TId, TValue> reader, IWrite<TId, TValue> writer)
    {
        _reader = reader;
        _writer = writer;
    }

    public TValue this[TId id]
    {
        get => _reader[id];
        set => _writer[id] = value;
    }

    public void Write(TId id, TValue value) => _writer.Write(id, value);
    public TValue Read(TId id) => _reader.Read(id);
    public bool Contains(TId id) => _reader.Contains(id);
}
public interface IReadWrite<in TId, TValue> : IRead<TId, TValue>, IWrite<TId, TValue>
{
    TValue this[TId id] { get; set; } // ?
}
public interface IReadWrite<in TId> : IRead<TId>, IWrite<TId> { }