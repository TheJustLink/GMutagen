using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v9.Contracts.Resolving.Nodes.From.Cache;


public class FromCollectionCache : IContractResolverNode
{
    private readonly IServiceCollection _cache;

    public FromCollectionCache(IServiceCollection cache)
    {
        _cache = cache;
    }

    public bool Resolve(Context context)
    {
        var provider = _cache.BuildServiceProvider();
        
        if (context.Key == null)
            return false;

        var instance = provider.GetKeyedService(context.Type, context.Key);

        context.Instance = instance;
        return true;
    }
}