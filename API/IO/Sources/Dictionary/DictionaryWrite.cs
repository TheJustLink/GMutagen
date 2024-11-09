using System.Collections.Generic;

namespace GMutagen.IO.Sources.Dictionary;

public class DictionaryWrite<TId, TValue> : IWrite<TId, TValue> where TId : notnull
{
    private readonly IDictionary<TId, TValue> _dictionary;
    public DictionaryWrite(IDictionary<TId, TValue> dictionary) => _dictionary = dictionary;

    public TValue this[TId id]
    {
        set => Write(id, value);
    }
    public void Write(TId id, TValue value) => _dictionary[id] = value;
}