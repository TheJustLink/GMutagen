namespace GMutagen.v9.Contracts.Resolving.Nodes.From;

public abstract class RecursiveContractResolverNode : IContractResolverNode
{
    protected IContractResolverNode Resolver;
    public abstract bool Resolve(Context context);
}