using System.Collections.Generic;
using System.Linq;

namespace GMutagen.v8.Test;

public class CompositeContractResolverChain : IContractResolverChain
{
    private readonly List<IContractResolverChain> _resolvers;
    public CompositeContractResolverChain(params IContractResolverChain[] resolvers)
    {
        _resolvers = new List<IContractResolverChain>(resolvers);
    }

    public CompositeContractResolverChain Add(IContractResolverChain resolver)
    {
        _resolvers.Add(resolver);
        return this;
    }

    public bool Resolve(ContractResolverContext context)
    {
        return _resolvers.Any(resolver => resolver.Resolve(context));
    }
}