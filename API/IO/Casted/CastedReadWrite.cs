namespace GMutagen.IO.Casted;

public class CastedReadWrite<TId, TValue> : IReadWrite<TId, TValue> where TId : notnull
{
    private readonly IRead<TId, object> _reader;
    private readonly IWrite<TId, object> _writer;

    public CastedReadWrite(IReadWrite<TId, object> readWrite)
        : this(readWrite, readWrite) { }
    public CastedReadWrite(IRead<TId, object> reader, IWrite<TId, object> writer)
    {
        _reader = reader;
        _writer = writer;
    }

    public int Count => _reader.Count;
    public TValue this[TId id]
    {
        get => (TValue)_reader[id];
        set => _writer[id] = value!;
    }

    public void Write(TId id, TValue value) => _writer.Write(id, value!);
    public TValue Read(TId id) => (TValue)_reader.Read(id);
    public bool Contains(TId id) => _reader.Contains(id);
}