using System;

using GMutagen.Generators;
using GMutagen.Objects;

namespace GMutagen.Contracts.Resolving.Nodes;

public class ContractResolverFromDescriptor<TContractId> : IContractResolverNode
    where TContractId : notnull
{
    private readonly IContractResolverNode _implementationTypeResolver;
    private readonly IGenerator<TContractId> _contractIdGenerator;
    public ContractResolverFromDescriptor(IContractResolverNode implementationTypeResolver, IGenerator<TContractId> contractIdGenerator)
    {
        _implementationTypeResolver = implementationTypeResolver;
        _contractIdGenerator = contractIdGenerator;
    }

    public bool Resolve(ContractResolverContext context)
    {
        var implementation = context.Contract.Implementation;
        var implementationType = context.Contract.ImplementationType!;

        if (implementation is not null)
        {
            DefineContractForObject(context.Services, implementation, context.Contract.Type);

            context.Result = implementation;
            return true;
        }

        if (context.Contract.ImplementationType is null)
            return false;

        var implementationContract = new ContractDescriptor(implementationType);
        var implementationContext = new ContractResolverContext(implementationContract, context.Services)
        {
            Key = context.Key,
            Attributes = context.Attributes
        };

        if (_implementationTypeResolver.Resolve(implementationContext) is false || implementationContext.Result is null)
            return false;

        context.Result = implementationContext.Result;
        return true;
    }
    private void DefineContractForObject(ContextServices services, object implementation, Type contractType)
    {
        var hasObject = services.TryGet<ObjectValue<TContractId>>(out var objectValue);
        if (hasObject is false) return;

        var cache = services.Get<ContractRuntimeCache<TContractId>>()!;
        objectValue[contractType] = cache.TryGetByImplementation(implementation, out var contractId)
            ? contractId
            : _contractIdGenerator.Generate();
    }
}