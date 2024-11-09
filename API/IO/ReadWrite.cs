namespace GMutagen.IO;

public class ReadWrite<TId, TValue> : IReadWrite<TId, TValue> where TId : notnull
{
    private readonly IRead<TId, TValue> _reader;
    private readonly IWrite<TId, TValue> _writer;

    public ReadWrite(IRead<TId, TValue> reader, IWrite<TId, TValue> writer)
    {
        _reader = reader;
        _writer = writer;
    }

    public int Count => _reader.Count;
    public TValue this[TId id]
    {
        get => _reader[id];
        set => _writer[id] = value;
    }

    public void Write(TId id, TValue value) => _writer.Write(id, value);
    public TValue Read(TId id) => _reader.Read(id);
    public bool Contains(TId id) => _reader.Contains(id);
}