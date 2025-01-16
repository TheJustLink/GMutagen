using System.Collections.Generic;
using GMutagen.v9.IO.Factories.Interfaces;
using GMutagen.v9.IO.Interfaces;

namespace GMutagen.v9.IO.Sources.Dictionary;

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