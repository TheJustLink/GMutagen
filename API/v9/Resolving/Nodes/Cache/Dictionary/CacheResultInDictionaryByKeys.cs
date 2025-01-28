using System.Collections.Generic;
using GMutagen.v9.Logging.Logger.Common;
using GMutagen.v9.Logging.Logger.Interfaces;
using GMutagen.v9.Logging.Messages.Realizations.Cache;
using GMutagen.v9.Resolving.Contexts.Key;
using GMutagen.v9.Resolving.Nodes.Decorator;
using GMutagen.v9.Resolving.Nodes.Interfaces;

namespace GMutagen.v9.Resolving.Nodes.Cache.Dictionary;

public class CacheResultInDictionaryByKeys(IResolverNode resolver, Dictionary<object, object> cache, KeyType[] keys, ILogger<Global> logger)
    : RecursiveResolverNode(resolver)
{
    public override bool Resolve(Contexts.Context context)
    {
        var success = Resolver.Resolve(context);
        if (!success)
        {
            logger.LogInfo(new CanNotCache(context, this));
            return false;
        }
        
        logger.LogInfo(new CachedWithKeysInDictionary(context, cache, this));
        Cache(context);

        return success;
    }

    private void Cache(Contexts.Context context)
    {
        foreach (var keyType in keys)
        {
            var success = context.TryGetKey<object>(keyType, out var key);
            if (success is false)
                continue;

            cache[key] = context.Instance!;
        }
    }
}