using GMutagen.v9.Contracts.Resolving.Nodes.From;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Cache;

public class CacheResultInContext : RecursiveContractResolverNode
{
    public override bool Resolve(Context context)
    {
        var success = Resolver.Resolve(context);
        if (success)
            context.Cache[context.Key!] = context.Instance!;

        return success;
    }
}