using System.Collections.Generic;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Nodes.From;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Cache;

public class CacheResultInDictionaryByKeys(IResolverNode resolver, Dictionary<object?, object> cache)
    : RecursiveResolverNode(resolver)
{
    public override bool Resolve(Context context)
    {
        var success = Resolver.Resolve(context);
        if (success)
            Cache(context);

        return success;
    }
    
    private void Cache(Context context)
    {
        if (context.Keys == null)
            return;

        foreach (var key in context.Keys)
            cache[key] = context.Instance!;
    }
}