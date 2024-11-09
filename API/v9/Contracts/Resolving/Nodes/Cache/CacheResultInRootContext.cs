using GMutagen.v9.Contracts.Resolving.Nodes.From;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Cache;

public class CacheResultInRootContext : RecursiveContractResolverNode
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
            currentContext.Cache[context.Key!] = context.Instance!;

        return success;
    }
}