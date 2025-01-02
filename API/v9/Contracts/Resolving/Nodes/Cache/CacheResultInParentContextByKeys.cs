using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Nodes.From;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Cache;

public class CacheResultInParentContextByKeys(IResolverNode resolver) : RecursiveResolverNode(resolver)
{
    public override bool Resolve(Context context)
    {
        var success = Resolver.Resolve(context);
        if (success)
            Cache(context.ParentContext!, context);

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