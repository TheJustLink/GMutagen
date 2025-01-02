using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Nodes.From;
using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Cache;


public class CacheResultInCollection(IResolverNode resolver, IServiceCollection cache)
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
        cache.AddSingleton(context.Type, context.Instance!);
    }

    private void CacheWithKeys(Context context)
    {
        foreach (var key in context.Keys!)
            cache.AddKeyedSingleton(context.Type, key, context.Instance!);
    }
}