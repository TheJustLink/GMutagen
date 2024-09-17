using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v8.Contracts.Resolving.Nodes;

public class ContractResolverContext
{
    public readonly ContractDescriptor Contract;
    public readonly IServiceCollection Services;

    public object? Key;
    public object? Result;

    public CustomAttributeData[]? Attributes;

    public ContractResolverContext(ContractDescriptor contract, IServiceCollection services)
    {
        Contract = contract;
        Services = services;
    }

    public IServiceProvider BuildServiceProvider() => Services.BuildServiceProvider();
}