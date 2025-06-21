using System;
using GMutagen.v9.Logging.Messages.Realizations;
using GMutagen.v9.Resolving.Contexts.Key;
using GMutagen.v9.Resolving.Nodes.Interfaces;
using Logger.Logger.Common;
using Logger.Logger.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v9.Resolving.Nodes.From.Cache.Collection;

public class FromCollectionCache(IServiceCollection cache, ILogger<Global> logger, KeyType[]? keys = null)
    : IResolverNode
{
    public bool Resolve(Contexts.Context context)
    {
        var provider = cache.BuildServiceProvider();

        if (keys == null)
            return TryResolve(provider, context);

        return TryResolveKeyed(provider, context);
    }

    private bool TryResolve(ServiceProvider provider, Contexts.Context context)
    {
        var keyType = KeyType.DeclaredType;
        var success = context.TryGetKey<Type>(keyType, out var type);
        if (success is false)
        {
            logger.LogWarning(new KeysDoNotContains(context.Keys, keyType, this));
            return false;
        }

        var instance = provider.GetService(type);

        if (instance is null)
        {
            logger.LogWarning(new CanNotResolveFromServicesCache(provider, context, this));
            return false;
        }
        
        context.Instance = instance;
        logger.LogInfo(new SuccessfullyResolved(context, this));
        return true;
    }


    private bool TryResolveKeyed(ServiceProvider provider, Contexts.Context context)
    {
        var keyType = KeyType.DeclaredType;
        var success = context.TryGetKey<Type>(keyType, out var type);
        if (success is false)
        {
            logger.LogWarning(new KeysDoNotContains(context.Keys, keyType, this));
            return false;
        }

        foreach (var key in keys)
        {
            var instance = provider.GetKeyedService(type, key);
            if (instance is null)
            {
                logger.LogWarning(new CanNotResolveFromServicesCache(provider, context, this));
                continue;
            }

            context.Instance = instance;
            return true;
        }

        return false;
    }
}