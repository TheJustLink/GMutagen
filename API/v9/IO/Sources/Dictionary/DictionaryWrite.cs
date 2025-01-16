using System.Collections.Generic;
using GMutagen.v9.IO.Interfaces;

namespace GMutagen.v9.IO.Sources.Dictionary;

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