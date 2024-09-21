using System;

using GMutagen.v8.Contracts.Resolving.Attributes;
using GMutagen.v8.Extensions;
using GMutagen.v8.Generators;
using GMutagen.v8.IO;
using GMutagen.v8.Values;
using GMutagen.v8.IO.Casted;

namespace GMutagen.v8.Contracts.Resolving.Nodes;

public class ValueResolverFromStorage<TSlotId, TValueId> : IContractResolverNode where TSlotId : notnull
    where TValueId : notnull
{
    private readonly IContractResolverNode _storageResolver;
    private readonly IGenerator<TValueId> _valueIdGenerator;
    public ValueResolverFromStorage(IContractResolverNode storageResolver, IGenerator<TValueId> valueIdGenerator)
    {
        _storageResolver = storageResolver;
        _valueIdGenerator = valueIdGenerator;
    }

    public bool Resolve(ContractResolverContext context)
    {
        if (context.Contract.Type.IsAssignableTo(typeof(IValue)) is false) return false;
        if (context.Key is not TSlotId slotId) return false;

        return ResolveValue(context, slotId);
    }

    private bool ResolveValue(ContractResolverContext context, TSlotId slotId)
    {
        if (ResolveStorage(context, out var storage) is false) return false;

        var hasContract = context.Services.TryGet<ContractValue<TSlotId, TValueId>>(out var contractValue);
        if (hasContract is false) return false;

        var valueType = GetValueType(context);
        context.Result = CreateExternalValue(storage!, contractValue, slotId, valueType);

        return true;
    }
    private bool ResolveStorage(ContractResolverContext context, out IReadWrite<TValueId, object>? storage)
    {
        storage = null;

        // TODO: Replace this boilerplate shit to own IServiceProvider wrapper (node resolving under hood)
        var valuesStorageType = typeof(IReadWrite<TValueId, object>);
        var storageContract = new ContractDescriptor(valuesStorageType);
        var storageResolverContext = new ContractResolverContext(storageContract, context.Services);

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

    private Type GetValueType(ContractResolverContext context)
    {
        return context.Contract.Type.GenericTypeArguments[0];
    }
    private object CreateExternalValue(IReadWrite<TValueId, object> storage, ContractValue<TSlotId, TValueId> contractValue, TSlotId slotId, Type valueType)
    {
        var valueId = GetValueId(contractValue, slotId);
        var valueFactory = CreateValueFactory(valueType);

        return valueFactory.Create(valueId, storage);
    }
    private TValueId GetValueId(ContractValue<TSlotId, TValueId> contractValue, TSlotId slotId)
    {
        if (contractValue.Slots.TryGetValue(slotId, out var valueId) is false)
        {
            valueId = _valueIdGenerator.Generate();
            contractValue.Slots[slotId] = valueId;
        }

        return valueId;
    }
    private IValueFactory CreateValueFactory(Type valueType)
    {
        var factoryType = typeof(ExternalValueFactory<>).MakeGenericType(typeof(TSlotId), typeof(TValueId), valueType)!;
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
                storage[id] = default(TValue)!; //TODO: Fix boxing

            var castedStorage = new CastedReadWrite<TValueId, TValue>(storage);
            return new ExternalValue<TValueId, TValue>(id, castedStorage);
        }
    }
}