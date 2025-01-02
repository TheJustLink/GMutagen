using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Contracts.Resolving.Nodes.From.Cache;

public class FromRootContextCache : IResolverNode
{
    public bool Resolve(Context context)
    {
        if (context.Keys == null)
            return false;
        
        var parentContext = context.ParentContext;
        var currentContext = context;

        while (parentContext != null)
        {
            currentContext = parentContext;
            parentContext = parentContext.ParentContext;
        }
        
        return TryResolve(currentContext, context);
    }
    
    private bool TryResolve(Context parentContext, Context context)
    {
        var cache = parentContext.Cache;
        
        foreach (var key in context.Keys!)
        {
            if (cache.TryGetValue(key, out var instance) is false)
                continue;
            
            context.Instance = instance;
            return true;
        }

        return false;
    }
}