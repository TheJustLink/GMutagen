namespace GMutagen.v9.Contracts.Resolving.Nodes.From.Cache;

public class FromAllContextsCache : IContractResolverNode
{
    public bool Resolve(Context context)
    {
        var currentContext = context;
        var key = context.Key;

        if (key == null)
            return false;
        
        while (currentContext != null)
        {
            if (currentContext.Cache.TryGetValue(key, out var instance))
            {
                context.Instance = instance;
                return true;
            }

            currentContext = currentContext.ParentContext;
        }
        
        return false;
    }
}