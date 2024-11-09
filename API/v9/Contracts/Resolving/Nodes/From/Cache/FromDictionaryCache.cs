using System.Collections.Generic;

namespace GMutagen.v9.Contracts.Resolving.Nodes.From.Cache;

public class FromDictionaryCache : IContractResolverNode
{
    private readonly Dictionary<object, object> _cache;

    public FromDictionaryCache(Dictionary<object, object> cache)
    {
        _cache = cache;
    }

    public bool Resolve(Context context)
    {
        if (context.Key == null || !_cache.TryGetValue(context.Key, out var instance))
            return false;

        context.Instance = instance;
        return true;
    }
}