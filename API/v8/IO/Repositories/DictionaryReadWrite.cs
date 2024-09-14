using System.Collections.Generic;

namespace GMutagen.v8.IO.Repositories;

public class DictionaryRead<TId, TValue> : IRead<TId, TValue> where TId : notnull
{
    private readonly IDictionary<TId, TValue> _memory;
    public DictionaryRead(IDictionary<TId, TValue> memory) => _memory = memory;

    public TValue this[TId id] => Read(id);
    public TValue Read(TId id) => _memory[id];
    public bool Contains(TId id) => _memory.ContainsKey(id);
}
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
public static class DictionaryReadWrite<TId, TValue> where TId : notnull
{
    public static IReadWrite<TId, TValue> Create() => Create(new Dictionary<TId, TValue>());
    public static IReadWrite<TId, TValue> Create(IDictionary<TId, TValue> dictionary)
    {
        return new ReadWrite<TId, TValue>(
            new DictionaryRead<TId, TValue>(dictionary),
            new DictionaryWrite<TId, TValue>(dictionary)
        );
    }
}