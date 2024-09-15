namespace GMutagen.v8.Contracts.Resolving.Nodes;

public interface IContractResolverNode
{
    bool Resolve(ContractResolverContext context);
}