using System;
using System.Linq;
using System.Reflection;

using GMutagen.v8.Extensions;
using GMutagen.v8.Generators;
using GMutagen.v8.Objects;

using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v8.Contracts.Resolving.Nodes;

public class ContractResolverFromConstructor<TContractId, TSlotId, TValueId> : IContractResolverNode
    where TContractId : notnull where TSlotId : notnull
{
    private readonly IServiceProvider _services;
    private readonly IContractResolverNode _parameterResolver;
    private readonly IGenerator<TContractId> _contractIdGenerator;
    public ContractResolverFromConstructor(IServiceProvider services, IContractResolverNode parameterResolver, IGenerator<TContractId> contractIdGenerator)
    {
        _services = services;
        _parameterResolver = parameterResolver;
        _contractIdGenerator = contractIdGenerator;
    }

    public bool Resolve(ContractResolverContext context)
    {
        var contextServiceProvider = context.BuildServiceProvider();
        
        var objectValue = contextServiceProvider.GetService<ObjectValue<TContractId>>();
        if (objectValue is null) return false;

        var contractType = context.Contract.Type;
        var contractValue = GetContractValue(objectValue, contractType);
        context.Services.AddSingleton(contractValue);

        var constructors = contractType.GetConstructors();
        return constructors.Any(constructor => ResolveConstructor(context, constructor));
    }
    private ContractValue<TSlotId, TValueId> GetContractValue(ObjectValue<TContractId> objectValue, Type contractType)
    {
        var contractId = GetContractId(objectValue, contractType);

        var contracts = _services.GetContractValues<TContractId, TSlotId, TValueId>();
        if (contracts.TryGet(contractId, out var contractValue) is false)
            contractValue = contracts[contractId] = new();

        return contractValue;
    }
    private TContractId GetContractId(ObjectValue<TContractId> objectValue, Type contractType)
    {
        if (objectValue.TryGetValue(contractType, out var contractId) is false)
        {
            contractId = _contractIdGenerator.Generate();
            objectValue[contractType] = contractId;
        }

        return contractId;
    }

    private bool ResolveConstructor(ContractResolverContext context, ConstructorInfo constructor)
    {
        var parameters = constructor.GetParameters();
        var resultParameters = new object[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameterInfo = parameters[i];
            context.Key = i;

            if (ResolveParameter(context, parameterInfo, out var parameter) is false)
                return false;

            resultParameters[i] = parameter!;
        }

        context.Result = constructor.Invoke(resultParameters);
        return true;
    }
    private bool ResolveParameter(ContractResolverContext context, ParameterInfo parameterInfo, out object? parameter)
    {
        parameter = default;

        var parameterContract = new ContractDescriptor(parameterInfo.ParameterType);
        var parameterContext = new ContractResolverContext(parameterContract, context.Services)
        {
            Key = context.Key,
            Attributes = parameterInfo.CustomAttributes.ToArray()
        };

        if (_parameterResolver.Resolve(parameterContext) is false || parameterContext.Result is null)
            return false;

        parameter = parameterContext.Result;

        return true;
    }
}