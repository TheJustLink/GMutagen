namespace GMutagen.Contracts.Resolving.Nodes;

public interface IContractResolverNode
{
    bool Resolve(ContractResolverContext context);
}