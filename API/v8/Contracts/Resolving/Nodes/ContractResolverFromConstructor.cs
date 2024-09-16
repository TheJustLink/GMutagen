using System;
using System.Linq;
using System.Reflection;
using GMutagen.v8.Contracts.Resolving.BuildServices;
using GMutagen.v8.Extensions;
using GMutagen.v8.Generators;
using GMutagen.v8.IO;
using GMutagen.v8.Objects;

using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v8.Contracts.Resolving.Nodes;

public class ContractResolverFromConstructor<TObjectId, TContractId> : IContractResolverNode where TObjectId : notnull
{
    private readonly IServiceProvider _services;
    private readonly IContractResolverNode _parameterResolver;
    private readonly IGenerator<TContractId> _contractIdGenerator;
    public ContractResolverFromConstructor(IServiceProvider services, IContractResolverNode parameterResolver, IGenerator<TContractId> contractIdGenerator)
    {
        _parameterResolver = parameterResolver;
        _services = services;
        _contractIdGenerator = contractIdGenerator;
    }

    public bool Resolve(ContractResolverContext context)
    {
        var buildServices = context.Services;
        var objectIdDescriptor = buildServices.GetRequiredService<ObjectId>();

        if (objectIdDescriptor.Value is not TObjectId objectId)
            return false;

        var objects = _services.GetRequiredService<IReadWrite<TObjectId, ObjectValue<TContractId>>>();
        if (objects.TryGet(objectId, out var objectValue) is false)
            objectValue = objects[objectId] = new ObjectValue<TContractId>();

        var contractType = context.Contract.Type;
        if (objectValue.TryGetValue(contractType, out var contractId) is false)
        {
            contractId = _contractIdGenerator.Generate();
            objectValue[contractType] = contractId;
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