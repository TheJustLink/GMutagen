using System.Collections.Generic;

namespace GMutagen.v8.IO.Sources.Dictionary;

public class DictionaryWrite<TId, TValue> : IWrite<TId, TValue> where TId : notnull
{
    private readonly IDictionary<TId, TValue> _memory;
    public DictionaryWrite(IDictionary<TId, TValue> memory) => _memory = memory;

    public TValue this[TId id]
    {
        set => Write(id, value);
    }
    public void Write(TId id, TValue value) => _memory[id] = value;
}