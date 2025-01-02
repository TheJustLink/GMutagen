using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Contracts.Resolving.Nodes.From.Cache;

public class FromAllContextsCaches : IResolverNode
{
    public bool Resolve(Context context)
    {
        var currentContext = context;

        while (currentContext != null)
        {
            if (TryResolve(currentContext, context))
                return true;

            currentContext = currentContext.ParentContext;
        }

        return false;
    }

    private bool TryResolve(Context currentContext, Context context)
    {
        var cache = currentContext.Cache;

        if (cache.TryGetValue(context.Type, out var instance) is false)
            return false;
        
        context.Instance = instance;
        return true;
    }
}