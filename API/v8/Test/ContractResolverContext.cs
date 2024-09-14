using System;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v8.Test;

public class ContractResolverContext
{
    public readonly ContractDescriptor Contract;
    public readonly IServiceCollection BuildServices;

    public object? Key;
    public object? Result;

    public CustomAttributeData[]? Attributes;

    public ContractResolverContext(ContractDescriptor contract, IServiceCollection services)
    {
        Contract = contract;
        BuildServices = services;
    }

    public IServiceProvider Services => BuildServices.BuildServiceProvider();
}