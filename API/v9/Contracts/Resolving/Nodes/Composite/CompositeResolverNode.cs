using System.Collections.Generic;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Nodes.Interfaces;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Composite;

public class CompositeResolverNode(params IResolverNode[] resolvers) : IResolverNode
{
    private readonly List<IResolverNode> _resolvers = new List<IResolverNode>(resolvers);

    public CompositeResolverNode Add(IResolverNode resolver)
    {
        _resolvers.Add(resolver);
        return this;
    }
    
    public CompositeResolverNode Remove(IResolverNode resolver)
    {
        _resolvers.Remove(resolver);
        return this;
    }

    public bool Resolve(Context context)
    {
        foreach (var resolver in _resolvers)
        {
           var success = resolver.Resolve(context);
           if (success)
               return success;
        }

        return false;
    }
}