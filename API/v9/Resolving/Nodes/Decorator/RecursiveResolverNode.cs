using GMutagen.v9.Resolving.Contexts;
using GMutagen.v9.Resolving.Nodes.Interfaces;

namespace GMutagen.v9.Resolving.Nodes.Decorator;

public abstract class RecursiveResolverNode(IResolverNode resolver) : IResolverNode
{
    protected readonly IResolverNode Resolver = resolver;

    public abstract bool Resolve(Context context);
}