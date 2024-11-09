namespace GMutagen.v9.Contracts.Resolving.Nodes.From.Cache;

public class FromContextCache : IContractResolverNode
{
    public bool Resolve(Context context)
    {
        if (context.Key == null || !context.Cache.TryGetValue(context.Key, out var instance))
            return false;

        context.Instance = instance;
        return true;
    }
}