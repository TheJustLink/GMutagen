using System.Collections.Generic;
using System.Linq;
using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Contracts.Resolving.Nodes;

public class CompositeResolverNode : IResolverNode
{
    private readonly List<IResolverNode> _resolvers;
    public CompositeResolverNode(params IResolverNode[] resolvers)
    {
        _resolvers = new List<IResolverNode>(resolvers);
    }

    public CompositeResolverNode Add(IResolverNode resolver)
    {
        _resolvers.Add(resolver);
        return this;
    }

    public bool Resolve(Context context)
    {
        return _resolvers.Any(resolver => resolver.Resolve(context));
    }
}