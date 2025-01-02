using System.Collections.Generic;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Contracts.Resolving.Nodes.From;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Cache;

public class CacheResultInDictionaryByKeys(IResolverNode resolver, Dictionary<object?, object> cache, KeyType[] keys)
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
        foreach (var keyType in keys)
        {
            var success = context.TryGetKey<object>(keyType, out var key);
            if(success is false)
                continue;
            
            cache[key] = context.Instance!;
        }
    }
}