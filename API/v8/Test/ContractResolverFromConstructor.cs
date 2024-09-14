using System;
using System.Linq;
using System.Reflection;

using GMutagen.v8.IO;
using GMutagen.v8.Objects;

using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v8.Test;

public class ContractResolverFromConstructor<TObjectId, TContractId> : IContractResolverChain
{
    private readonly IContractResolverChain _parameterResolver;
    private readonly IGenerator<TContractId> _contractIdGenerator;
    public ContractResolverFromConstructor(IContractResolverChain parameterResolver, IGenerator<TContractId> contractIdGenerator)
    {
        _parameterResolver = parameterResolver;
        _contractIdGenerator = contractIdGenerator;
    }

    public bool Resolve(ContractResolverContext context)
    {
        var services = context.Services;
        var objectIdDescriptor = services.GetRequiredService<ObjectId>();

        var objects = services.GetRequiredService<IReadWrite<TObjectId, IReadWrite<Type, TContractId>>>();
        if (objectIdDescriptor.Value is not TObjectId objectId)
            return false;

        var contractType = context.Contract.Type;
        var contracts = objects[objectId];
        TContractId contractId;
        
        if (contracts.Contains(contractType))
        {
            contractId = contracts[contractType];
        }
        else
        {
            contractId = _contractIdGenerator.Generate();
            contracts[contractType] = contractId;
        }
        context.BuildServices.AddSingleton(new ContractId(typeof(TContractId), contractId));

        var constructors = contractType.GetConstructors();
        return constructors.Any(constructor => ResolveConstructor(context, constructor));
    }

    private bool ResolveConstructor(ContractResolverContext context, ConstructorInfo constructor)
    {
        var parameters = constructor.GetParameters();
        var resultParameters = new object[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameterInfo = parameters[i];
            context.BuildServices.AddSingleton(new SlotId(i.GetType(), i));

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
        var parameterContext = new ContractResolverContext(parameterContract, context.BuildServices);
        parameterContext.Attributes = parameterInfo.CustomAttributes.ToArray();

        if (_parameterResolver.Resolve(parameterContext) is false || parameterContext.Result is null)
            return false;

        parameter = parameterContext.Result;

        return true;
    }
}