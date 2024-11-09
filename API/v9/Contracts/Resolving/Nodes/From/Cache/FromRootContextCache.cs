namespace GMutagen.v9.Contracts.Resolving.Nodes.From.Cache;

public class FromRootContextCache : IContractResolverNode
{
    public bool Resolve(Context context)
    {
        var parentContext = context.ParentContext;
        var currentContext = context;

        while (parentContext != null)
        {
            currentContext = parentContext;
            parentContext = parentContext.ParentContext;
        }
        
        if (context.Key == null || !currentContext.Cache.TryGetValue(context.Key, out var instance))
            return false;

        context.Instance = instance;
        return true;
    }
}