using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;

namespace GMutagen.v9.Contracts.Resolving.Nodes.From.Cache;

public class FromRootContextCache(KeyType[] keys) : IResolverNode
{
    public bool Resolve(Context context)
    {
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

        if (cache.TryGetValue(context.Type, out var instance) is false)
            return false;

        context.Instance = instance;
        return true;
    }
}