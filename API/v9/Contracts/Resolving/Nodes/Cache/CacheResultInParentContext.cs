using GMutagen.v9.Contracts.Resolving.Nodes.From;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Cache;

public class CacheResultInParentContext : RecursiveContractResolverNode
{
    public override bool Resolve(Context context)
    {
        var success = Resolver.Resolve(context);
        if (success)
            context.ParentContext.Cache[context.Key!] = context.Instance!;

        return success;
    }
}