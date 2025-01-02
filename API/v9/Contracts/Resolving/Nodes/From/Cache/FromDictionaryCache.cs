using System.Collections.Generic;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;

namespace GMutagen.v9.Contracts.Resolving.Nodes.From.Cache;

public class FromDictionaryCache(Dictionary<object?, object> cache, KeyType[] keys) : IResolverNode
{
    public bool Resolve(Context context)
    {
        return TryResolve(context);
    }

    private bool TryResolve(Context context)
    {
        if (cache.TryGetValue(context.Type, out var instance) is false)
            return false;

        context.Instance = instance;
        return true;
    }
}