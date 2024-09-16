using System;

using Microsoft.Extensions.DependencyInjection;

using GMutagen.v8.Contracts.Resolving.Attributes;
using GMutagen.v8.Contracts.Resolving.BuildServices;
using GMutagen.v8.Extensions;
using GMutagen.v8.Generators;
using GMutagen.v8.IO;
using GMutagen.v8.Values;
using GMutagen.v8.IO.Casted;

namespace GMutagen.v8.Contracts.Resolving.Nodes;

public class ValueResolverFromStorage<TContractId, TSlotId, TValueId> : IContractResolverNode
    where TContractId : notnull
    where TSlotId : notnull
    where TValueId : notnull
{
    private readonly IServiceProvider _services;
    private readonly IContractResolverNode _storageResolver;
    private readonly IGenerator<TValueId> _valueIdGenerator;

    public ValueResolverFromStorage(IServiceProvider services, IContractResolverNode storageResolver, IGenerator<TValueId> valueIdGenerator)
    {
        _services = services;
        _storageResolver = storageResolver;
        _valueIdGenerator = valueIdGenerator;
    }

    public bool Resolve(ContractResolverContext context)
    {
        if (context.Contract.Type.IsAssignableTo(typeof(IValue)) is false)
            return false;

        return ResolveStorage(context, out var storage) && Resolve(context, storage!);
    }

    private bool Resolve(ContractResolverContext context, IReadWrite<TValueId, object> valuesStorage)
    {
        var buildServices = context.Services;
        var contractIdDescriptor = buildServices.GetRequiredService<ContractId>();
        var slotIdDescriptor = buildServices.GetRequiredService<SlotId>();

        if (contractIdDescriptor.Value is not TContractId contractId || slotIdDescriptor.Value is not TSlotId slotId)
            return false;

        var contracts = _services.GetRequiredService<IReadWrite<TContractId, ContractValue<TSlotId, TValueId>>>();
        if (contracts.TryGet(contractId, out var contractValue) is false)
            contractValue = contracts[contractId] = new ContractValue<TSlotId, TValueId>();

        if (contractValue.TryGetValue(slotId, out var valueId) is false)
        {
            valueId = _valueIdGenerator.Generate();
            contractValue[slotId] = valueId;
        }

        var valueType = context.Contract.Type.GenericTypeArguments[0];
        var valueFactory = CreateValueFactory(valueType);

        context.Result = valueFactory.Create(valueId, valuesStorage);

        return true;
    }
    private bool ResolveStorage(ContractResolverContext context, out IReadWrite<TValueId, object>? storage)
    {
        storage = null;

        var valuesStorageType = typeof(IReadWrite<TValueId, object>);
        var storageContract = new ContractDescriptor(valuesStorageType);
        var storageResolverContext = new ContractResolverContext(storageContract, context.BuildServices);

        if (context.Attributes is not null)
        {
            var locationKey = context.Attributes.Get<ValueLocationAttribute>();
            if (locationKey is not null)
                storageResolverContext.Key = locationKey.AttributeType;
        }

        if (_storageResolver.Resolve(storageResolverContext) is false || storageResolverContext.Result is null)
            return false;

        storage = (storageResolverContext.Result as IReadWrite<TValueId, object>)!;
        return true;
    }
    private IValueFactory CreateValueFactory(Type valueType)
    {
        var factoryType = typeof(ExternalValueFactory<>).MakeGenericType(typeof(TContractId), typeof(TSlotId), typeof(TValueId), valueType)!;
        return (Activator.CreateInstance(factoryType) as IValueFactory)!;
    }


    private interface IValueFactory
    {
        object Create(TValueId id, IReadWrite<TValueId, object> storage);
    }
    private class ExternalValueFactory<TValue> : IValueFactory
    {
        public object Create(TValueId id, IReadWrite<TValueId, object> storage)
        {
            if (storage.Contains(id) is false)
                storage[id] = default(TValue)!;

            var castedStorage = new CastedReadWrite<TValueId, TValue>(storage);
            return new ExternalValue<TValueId, TValue>(id, castedStorage);
        }
    }
}