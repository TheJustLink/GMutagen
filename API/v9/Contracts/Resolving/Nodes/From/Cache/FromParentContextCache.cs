namespace GMutagen.v9.Contracts.Resolving.Nodes.From.Cache;

public class FromParentContextCache : IContractResolverNode
{
    public bool Resolve(Context context)
    {
        if (context.Key == null || !context.ParentContext.Cache.TryGetValue(context.Key, out var instance))
            return false;

        context.Instance = instance;
        return true;
    }
}