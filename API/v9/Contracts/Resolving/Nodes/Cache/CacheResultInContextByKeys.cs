using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Contracts.Resolving.Nodes.From;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Cache;

public class CacheResultInContextByKeys(IResolverNode resolver, KeyType[] keyTypes, int parentOffset) : RecursiveResolverNode(resolver)
{
    public override bool Resolve(Context context)
    {
        var parentContext = context.ParentContext;
        var currentContext = context;

        var index = 0;
        
        while (parentContext != null && index < parentOffset)
        {
            currentContext = parentContext;
            parentContext = parentContext.ParentContext;
            index++;
        }

        var success = Resolver.Resolve(context);
        if (success)
            Cache(currentContext, context);

        return success;
    }
    
    private void Cache(Context parentContext, Context context)
    {
        foreach (var keyType in keyTypes)
        {
            var success = context.TryGetKey<object>(keyType, out var key);
            if(success is false)
                continue;
            
            parentContext.Cache[key] = context.Instance!;
        }
    }
}