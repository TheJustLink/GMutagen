using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Nodes.From;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Cache;

public class CacheResultInRootContextByKeys(IResolverNode resolver) : RecursiveResolverNode(resolver)
{
    public override bool Resolve(Context context)
    {
        var parentContext = context.ParentContext;
        var currentContext = context;

        while (parentContext != null)
        {
            currentContext = parentContext;
            parentContext = parentContext.ParentContext;
        }

        var success = Resolver.Resolve(context);
        if (success)
            Cache(currentContext, context);

        return success;
    }
    
    private void Cache(Context parentContext, Context context)
    {
        if (context.Keys == null)
            return;

        foreach (var key in context.Keys)
            parentContext.Cache[key] = context.Instance!;
    }
}