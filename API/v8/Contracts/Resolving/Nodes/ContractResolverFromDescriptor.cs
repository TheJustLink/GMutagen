namespace GMutagen.v8.Contracts.Resolving.Nodes;

public class ContractResolverFromDescriptor : IContractResolverNode
{
    private readonly IContractResolverNode _implementationTypeResolver;
    public ContractResolverFromDescriptor(IContractResolverNode implementationTypeResolver)
    {
        _implementationTypeResolver = implementationTypeResolver;
    }

    public bool Resolve(ContractResolverContext context)
    {
        context.Result = context.Contract.Implementation;
        if (context.Result is not null)
            return true;

        if (context.Contract.ImplementationType is null)
            return false;

        var implementationContract = new ContractDescriptor(context.Contract.ImplementationType);
        var implementationContext = new ContractResolverContext(implementationContract, context.BuildServices);

        if (_implementationTypeResolver.Resolve(implementationContext) is false || implementationContext.Result is null)
            return false;

        context.Result = implementationContext.Result;
        return true;
    }
}