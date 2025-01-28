using GMutagen.v9.Logging.Logger.Common;
using GMutagen.v9.Logging.Logger.Interfaces;
using GMutagen.v9.Logging.Messages.Realizations.Cache;
using GMutagen.v9.Resolving.Contexts.Key;
using GMutagen.v9.Resolving.Nodes.Decorator;
using GMutagen.v9.Resolving.Nodes.Interfaces;

namespace GMutagen.v9.Resolving.Nodes.Cache.Context;

public class CacheResultInContextByKeys(IResolverNode resolver, KeyType[] keyTypes, int parentOffset, ILogger<Global> logger) : RecursiveResolverNode(resolver)
{
    public override bool Resolve(Contexts.Context context)
    {
        var success = Resolver.Resolve(context);
        if (!success)
        {
            logger.LogInfo(new CanNotCache(context, this));
            return false;
        }
        
        var parentContext = context.ParentContext;
        var currentContext = context;

        var index = 0;
        
        while (parentContext != null && index < parentOffset)
        {
            currentContext = parentContext;
            parentContext = parentContext.ParentContext;
            index++;
        }

        logger.LogInfo(new CachedWithKeysInContext(context, currentContext, this));
        Cache(currentContext, context);

        return success;
    }
    
    private void Cache(Contexts.Context parentContext, Contexts.Context context)
    {
        foreach (var keyType in keyTypes)
        {
            var success = context.TryGetKey<object>(keyType, out var key);
            if(success is false)
                continue;
            
            parentContext.Cache[key] = context.Instance!;
        }
    }
}