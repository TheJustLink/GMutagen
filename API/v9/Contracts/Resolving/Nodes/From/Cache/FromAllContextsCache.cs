using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;

namespace GMutagen.v9.Contracts.Resolving.Nodes.From.Cache;

public class FromAllContextsCache : IResolverNode
{
    public bool Resolve(Context context)
    {
        var currentContext = context;
        var keys = context.Keys;

        if (keys == null)
            return false;

        while (currentContext != null)
        {
            if (TryResolve(currentContext, keys, context))
                return true;
            
            currentContext = currentContext.ParentContext;
        }

        return false;
    }

    private bool TryResolve(Context currentContext, Keys keys, Context context)
    {
        var cache = currentContext.Cache;
        
        foreach (var key in keys)
        {
            if (cache.TryGetValue(key, out var instance) is false)
                continue;
            
            context.Instance = instance;
            return true;
        }

        return false;
    }
}