using System.Collections.Generic;
using System.Linq;

namespace GMutagen.Contracts.Resolving.Nodes;

public class CompositeContractResolverNode : IContractResolverNode
{
    private readonly List<IContractResolverNode> _resolvers;
    public CompositeContractResolverNode(params IContractResolverNode[] resolvers)
    {
        _resolvers = new List<IContractResolverNode>(resolvers);
    }

    public CompositeContractResolverNode Add(IContractResolverNode resolver)
    {
        _resolvers.Add(resolver);
        return this;
    }

    public bool Resolve(ContractResolverContext context)
    {
        return _resolvers.Any(resolver => resolver.Resolve(context));
    }
}