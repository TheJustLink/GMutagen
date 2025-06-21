using System;
using GMutagen.v9.Logging.Messages.Realizations.Cache;
using GMutagen.v9.Resolving.Contexts.Key;
using GMutagen.v9.Resolving.Nodes.Decorator;
using GMutagen.v9.Resolving.Nodes.Interfaces;
using Logger.Logger.Common;
using Logger.Logger.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v9.Resolving.Nodes.Cache.Collection;

public class CacheResultInCollection(IResolverNode resolver, IServiceCollection cache, KeyType[] keyTypes, ILogger<Global> logger)
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

        if (context.Keys == null)
        {
            CacheWithoutKeys(context);
            logger.LogInfo(new CachedWithoutKeysInCollection(context, cache, this));
        }
        else
        {
            CacheWithKeys(context);
            logger.LogInfo(new CachedWithKeysInCollection(context, cache, this));
        }

        return success;
    }

    private void CacheWithoutKeys(Contexts.Context context)
    {
        var success = context.TryGetKey<Type>(KeyType.DeclaredType, out var type);
        if(success is false)
            return;
            
        cache.AddSingleton(type, context.Instance!);
    }

    private void CacheWithKeys(Contexts.Context context)
    {
        var success = context.TryGetKey<Type>(KeyType.DeclaredType, out var type);
        if(success is false)
            return;

        foreach (var keyType in keyTypes)
        {
            success = context.TryGetKey<object>(keyType, out var key);
            if(success is false)
                continue;
            
            cache.AddKeyedSingleton(type, key, context.Instance!);
        }
    }
}