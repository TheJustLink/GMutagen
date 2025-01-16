using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Contracts.Resolving.Nodes.Interfaces;
using GMutagen.v9.Logging.Logger.Common;
using GMutagen.v9.Logging.Logger.Interfaces;
using GMutagen.v9.Logging.Messages.Resolve;

namespace GMutagen.v9.Contracts.Resolving.Nodes.From.Cache.Context;

public class FromContextCache(KeyType[] keys, int parentOffset, ILogger<Global> logger) : IResolverNode
{
    public bool Resolve(Contexts.Context context)
    {
        var parentContext = context.ParentContext;
        var currentContext = context;
        var index = 0;

        while (parentContext != null && index < parentOffset)
        {
            currentContext = parentContext;
            parentContext = parentContext.ParentContext;
            index++;
        }

        return TryResolve(currentContext, context);
    }

    private bool TryResolve(Contexts.Context currentContext, Contexts.Context context)
    {
        var cache = currentContext.Cache;

        if (cache.TryGetValue(context.Type, out var instance) is false)
        {
            logger.LogWarning(new CanNotResolveFromContextCache(currentContext, context, this));
            return false;
        }

        logger.LogInfo(new ResolvedFromContextCache(currentContext, context, this));
        context.Instance = instance;
        return true;
    }
}