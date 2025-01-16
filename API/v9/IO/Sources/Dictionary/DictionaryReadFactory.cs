using System.Collections.Generic;
using GMutagen.v9.IO.Factories.Interfaces;
using GMutagen.v9.IO.Interfaces;

namespace GMutagen.v9.IO.Sources.Dictionary;

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