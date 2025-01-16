using GMutagen.v9.Contracts.Resolving.Nodes.Interfaces;
using GMutagen.v9.Logging.Logger.Common;
using GMutagen.v9.Logging.Logger.Interfaces;
using GMutagen.v9.Logging.Messages.Resolve;

namespace GMutagen.v9.Contracts.Resolving.Nodes.From.Cache.Context;

public class FromAllContextsCaches(ILogger<Global> logger) : IResolverNode
{
    public bool Resolve(Contexts.Context context)
    {
        var currentContext = context;

        while (currentContext != null)
        {
            if (TryResolve(currentContext, context))
            {
                logger.LogInfo(new ResolvedFromContextCache(currentContext, context, this));
                return true;
            }

            logger.LogWarning(new CanNotResolveFromContextCache(currentContext, context, this));
            currentContext = currentContext.ParentContext;
        }

        return false;
    }

    private bool TryResolve(Contexts.Context currentContext, Contexts.Context context)
    {
        var cache = currentContext.Cache;

        if (cache.TryGetValue(context.Type, out var instance) is false)
            return false;

        context.Instance = instance;
        return true;
    }
}