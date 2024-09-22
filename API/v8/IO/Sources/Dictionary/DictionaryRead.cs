using System.Collections.Generic;

namespace GMutagen.v8.IO.Sources.Dictionary;

public class DictionaryRead<TId, TValue> : IRead<TId, TValue> where TId : notnull
{
    private readonly IDictionary<TId, TValue> _dictionary;
    public DictionaryRead(IDictionary<TId, TValue> dictionary) => _dictionary = dictionary;

    public int Count => _dictionary.Count;
    public TValue this[TId id] => Read(id);

    public TValue Read(TId id) => _dictionary[id];
    public bool Contains(TId id) => _dictionary.ContainsKey(id);
}