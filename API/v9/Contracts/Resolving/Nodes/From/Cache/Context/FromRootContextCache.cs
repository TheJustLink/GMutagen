using GMutagen.v9.Contracts.Resolving.Nodes.Interfaces;
using GMutagen.v9.Logging.Logger.Common;
using GMutagen.v9.Logging.Logger.Interfaces;
using GMutagen.v9.Logging.Messages.Resolve;

namespace GMutagen.v9.Contracts.Resolving.Nodes.From.Cache.Context;

public class FromRootContextCache(ILogger<Global> logger) : IResolverNode
{
    public bool Resolve(Contexts.Context context)
    {
        var parentContext = context.ParentContext;
        var currentContext = context;

        while (parentContext != null)
        {
            currentContext = parentContext;
            parentContext = parentContext.ParentContext;
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