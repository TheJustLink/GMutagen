using System;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Contracts.Resolving.Nodes.From;
using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Cache;

public class CacheResultInCollection(IResolverNode resolver, IServiceCollection cache, KeyType[] keyTypes)
    : RecursiveResolverNode(resolver)
{
    public override bool Resolve(Context context)
    {
        var success = Resolver.Resolve(context);
        if (!success)
            return false;

        if (context.Keys == null)
            CacheWithoutKeys(context);
        else
            CacheWithKeys(context);

        return success;
    }

    private void CacheWithoutKeys(Context context)
    {
        var success = context.TryGetKey<Type>(KeyType.DeclaredType, out var type);
        if(success is false)
            return;
            
        cache.AddSingleton(type, context.Instance!);
    }

    private void CacheWithKeys(Context context)
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