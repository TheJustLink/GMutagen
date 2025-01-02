using System;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v9.Contracts.Resolving.Nodes.From.Cache;

public class FromCollectionCache(IServiceCollection cache, KeyType[]? keys = null) : IResolverNode
{
    public bool Resolve(Context context)
    {
        var provider = cache.BuildServiceProvider();

        if (keys == null)
            return TryResolve(provider, context);

        return TryResolveKeyed(provider, context);
    }

    private bool TryResolve(ServiceProvider provider, Context context)
    {
        var success = context.TryGetKey<Type>(KeyType.DeclaredType, out var type);
        if (success is false)
            return false;

        var instance = provider.GetService(type);

        if (instance is null)
            return false;

        context.Instance = instance;
        return true;
    }


    private bool TryResolveKeyed(ServiceProvider provider, Context context)
    {
        var success = context.TryGetKey<Type>(KeyType.DeclaredType, out var type);
        if (success is false)
            return false;

        foreach (var key in keys)
        {
            var instance = provider.GetKeyedService(type, key);
            if (instance is null)
                continue;

            context.Instance = instance;
            return true;
        }

        return false;
    }
}