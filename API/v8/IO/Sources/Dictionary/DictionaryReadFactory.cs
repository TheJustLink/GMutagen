using System.Collections.Generic;

namespace GMutagen.v8.IO.Sources.Dictionary;

public class DictionaryReadFactory : IReadFactory
{
    public IRead<TId, TValue> CreateRead<TId, TValue>() where TId : notnull
    {
        return CreateRead(new Dictionary<TId, TValue>());
    }
    public IRead<TId, TValue> CreateRead<TId, TValue>(IDictionary<TId, TValue> dictionary) where TId : notnull
    {
        return new DictionaryRead<TId, TValue>(dictionary);
    }
}