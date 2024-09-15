using System.Collections.Generic;

namespace GMutagen.v8.IO.Sources.Dictionary;

public class DictionaryReadWriteFactory : IReadWriteFactory
{
    public IReadWrite<TId, TValue> CreateReadWrite<TId, TValue>() where TId : notnull
    {
        return Create(new Dictionary<TId, TValue>());
    }
    public IReadWrite<TId, TValue> Create<TId, TValue>(IDictionary<TId, TValue> dictionary) where TId : notnull
    {
        return new ReadWrite<TId, TValue>(
            new DictionaryReadFactory().CreateRead(dictionary),
            new DictionaryWriteFactory().CreateWrite(dictionary)
        );
    }
}