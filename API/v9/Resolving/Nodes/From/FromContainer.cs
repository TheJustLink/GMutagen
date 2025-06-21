using System;
using GMutagen.v9.Extensions;
using GMutagen.v9.Logging.Messages.Realizations;
using GMutagen.v9.Resolving.Contexts;
using GMutagen.v9.Resolving.Contexts.Key;
using GMutagen.v9.Resolving.Nodes.Interfaces;
using Logger.Logger.Common;
using Logger.Logger.Interfaces;

namespace GMutagen.v9.Resolving.Nodes.From;

public class FromContainer(IServiceProvider provider, ILogger<Global> logger) : IResolverNode
{
    public bool Resolve(Context context)
    {
        var keyType = KeyType.DeclaredType;
        var success = context.TryGetKey<Type>(keyType, out var type);
        if (success is false)
        {
            logger.LogWarning(new KeysDoNotContains(context.Keys, keyType, this));
            return false;
        }

        if (context.Keys == null)
        {
            var instance = provider.GetService(type);
            context.Instance = instance;
            success = instance is not null;

            if (success)
            {
                logger.LogInfo(new SuccessfullyResolved(context, this));
                return true;
            }
        }
        else
        {

            foreach (var key in context.Keys)
            {
                var instance = provider.GetKeyedService(type, key);
                context.Instance = instance;

                if (instance is not null)
                {
                    logger.LogInfo(new SuccessfullyResolved(context, this));
                    return true;
                }
            }
        }

        logger.LogWarning(new CanNotResolveFromServicesCache(provider, context, this));
        return false;
    }
}