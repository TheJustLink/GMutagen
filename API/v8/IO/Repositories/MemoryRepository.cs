using System.Collections.Generic;

namespace GMutagen.v8.IO.Repositories;

public class MemoryRepository<TId, TValue> : IReadWrite<TId, TValue> where TId : notnull
{
    private readonly Dictionary<TId, TValue> _memory;

    public MemoryRepository() : this(new Dictionary<TId, TValue>())
    {
    }

    public MemoryRepository(Dictionary<TId, TValue> memory)
    {
        _memory = memory;
    }

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