using System;
using System.Linq;
using System.Reflection;

using GMutagen.v8.Extensions;
using GMutagen.v8.Generators;
using GMutagen.v8.Objects;

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
        var hasObject = context.Services.TryGet<ObjectValue<TContractId>>(out var objectValue);
        if (hasObject is false) return false;

        var contractType = context.Contract.Type;
        var contractId = GetContractId(objectValue, contractType);

        return ResolveFromCache(context, contractId)
            || ResolveFromInstantiation(context, contractId, contractType);
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

    private bool ResolveFromCache(ContractResolverContext context, TContractId contractId)
    {
        var cache = context.Services.Get<ContractRuntimeCache<TContractId>>()!;
        if (cache.TryGetById(contractId, out var implementation) is false)
            return false;

        context.Result = implementation;
        return true;
    }
    private bool ResolveFromInstantiation(ContractResolverContext context, TContractId contractId, Type contractType)
    {
        var contractValue = GetContractValue(contractId);
        context.Services.Set(contractValue);

        var constructors = contractType.GetConstructors();
        var isResolved = constructors.Any(constructor => ResolveConstructor(context, constructor));

        if (isResolved) PutToCache(context.Services, contractId, context.Result!);

        return isResolved;
    }
    private ContractValue<TSlotId, TValueId> GetContractValue(TContractId contractId)
    {
        var contracts = _services.GetContractValues<TContractId, TSlotId, TValueId>();
        if (contracts.TryGet(contractId, out var contractValue) is false)
            contractValue = contracts[contractId] = new();

        return contractValue;
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
    private void PutToCache(ContextServices services, TContractId id, object implementation)
    {
        var cache = services.Get<ContractRuntimeCache<TContractId>>()!;
        cache.Add(id, implementation);
    }
}