using GMutagen.v9.Contracts.Resolving.Nodes.From;
using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Cache;


public class CacheResultInCollection : RecursiveContractResolverNode
{
    private readonly IServiceCollection _cache;
    
    public CacheResultInCollection(IServiceCollection cache)
    {
        _cache = cache;
    }

    public override bool Resolve(Context context)
    {
        var success = Resolver.Resolve(context);
        if (!success)
            return false;

        if (context.Key != null)
            _cache.AddKeyedSingleton(context.Type, context.Key, context.Instance!);
        else
            _cache.AddSingleton(context.Type, context.Instance!);

        return success;
    }
}