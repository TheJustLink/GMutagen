using System.Collections.Generic;
using GMutagen.v9.Contracts.Resolving.Nodes.From;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Cache;

public class CacheResultInDictionary : RecursiveContractResolverNode
{
    private readonly Dictionary<object, object> _cache;
    
    public CacheResultInDictionary(Dictionary<object, object> cache)
    {
        _cache = cache;
    }

    public override bool Resolve(Context context)
    {
        var success = Resolver.Resolve(context);
        if (success)
            _cache[context.Key!] = context.Instance!;

        return success;
    }
}