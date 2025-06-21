using System.Collections.Generic;
using GMutagen.v9.Logging.Messages.Realizations;
using GMutagen.v9.Resolving.Nodes.Interfaces;
using Logger.Logger.Common;
using Logger.Logger.Interfaces;

namespace GMutagen.v9.Resolving.Nodes.From.Cache.Dictionary;

public class FromDictionaryCache(Dictionary<object, object> cache, ILogger<Global> logger) : IResolverNode
{
    public bool Resolve(Contexts.Context context)
    {
        return TryResolve(context);
    }

    private bool TryResolve(Contexts.Context context)
    {
        if (cache.TryGetValue(context.Type, out var instance) is false)
        {
            logger.LogWarning(new CanNotResolveFromDictionaryCache(cache, context, this));
            return false;
        }

        context.Instance = instance;
        return true;
    }
}