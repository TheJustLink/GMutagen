using System.Reflection;

namespace GMutagen.Contracts.Resolving;

public class ContractResolverContext
{
    public readonly ContractDescriptor Contract;
    public readonly ContextServices Services;

    public object? Key;
    public object? Result;

    public CustomAttributeData[]? Attributes;

    public ContractResolverContext(ContractDescriptor contract, ContextServices services)
    {
        Contract = contract;
        Services = services;
    }
}