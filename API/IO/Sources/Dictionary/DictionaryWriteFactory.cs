using System.Collections.Generic;

namespace GMutagen.IO.Sources.Dictionary;

public class DictionaryWriteFactory : IWriteFactory
{
    public IWrite<TId, TValue> CreateWrite<TId, TValue>() where TId : notnull
    {
        return CreateWrite(new Dictionary<TId, TValue>());
    }
    public IWrite<TId, TValue> CreateWrite<TId, TValue>(IDictionary<TId, TValue> dictionary) where TId : notnull
    {
        return new DictionaryWrite<TId, TValue>(dictionary);
    }
}