using System.Collections.Generic;
using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Contracts.Resolving.Nodes.From.Cache;

public class FromDictionaryCache : IResolverNode
{
    private readonly Dictionary<object?, object> _cache;

    public FromDictionaryCache(Dictionary<object?, object> cache)
    {
        _cache = cache;
    }

    public bool Resolve(Context context)
    {
        if (context.Keys == null)
            return false;
        
        return TryResolve(context);
    }
    
    private bool TryResolve(Context context)
    {
        foreach (var key in context.Keys!)
        {
            if (_cache.TryGetValue(key, out var instance) is false)
                continue;
            
            context.Instance = instance;
            return true;
        }

        return false;
    }
}