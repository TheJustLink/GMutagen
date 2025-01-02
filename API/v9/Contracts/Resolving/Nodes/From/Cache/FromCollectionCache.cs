using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v9.Contracts.Resolving.Nodes.From.Cache;


public class FromCollectionCache : IResolverNode
{
    private readonly IServiceCollection _cache;

    public FromCollectionCache(IServiceCollection cache)
    {
        _cache = cache;
    }

    public bool Resolve(Context context)
    {
        var provider = _cache.BuildServiceProvider();
        
        if (context.Keys == null)
            return false;

        return TryResolve(provider, context.Keys, context);
    }
    
    private bool TryResolve(ServiceProvider provider, Keys keys, Context context)
    {
        foreach (var key in keys)
        {
            var instance = provider.GetKeyedService(context.Type, key);
            if (instance is null)
                continue;
            
            context.Instance = instance;
            return true;
        }

        return false;
    }
}