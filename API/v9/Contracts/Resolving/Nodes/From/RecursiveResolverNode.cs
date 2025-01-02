using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Contracts.Resolving.Nodes.From;

public abstract class RecursiveResolverNode(IResolverNode resolver) : IResolverNode
{
    protected readonly IResolverNode Resolver = resolver;

    public abstract bool Resolve(Context context);
}