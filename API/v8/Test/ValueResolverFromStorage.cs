using System;

using GMutagen.v8.IO;
using GMutagen.v8.Objects;
using GMutagen.v8.Values;

using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v8.Test;

public class ValueResolverFromStorage<TContractId, TSlotId, TValueId> : IContractResolverChain
{
    private readonly IContractResolverChain _storageResolver;
    private readonly IGenerator<TValueId> _valueIdGenerator;
    public ValueResolverFromStorage(IContractResolverChain storageResolver, IGenerator<TValueId> valueIdGenerator)
    {
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
        var services = context.Services;
        var contractIdDescriptor = services.GetRequiredService<ContractId>();
        var slotIdDescriptor = services.GetRequiredService<SlotId>();

        var contracts = services.GetRequiredService<IReadWrite<TContractId, IReadWrite<TSlotId, TValueId>>>();
        if (contractIdDescriptor.Value is not TContractId contractId || slotIdDescriptor.Value is not TSlotId slotId)
            return false;

        var slots = contracts.Read(contractId);
        TValueId valueId;
        if (slots.Contains(slotId))
        {
            valueId = slots[slotId];
        }
        else
        {
            valueId = _valueIdGenerator.Generate();
            slots[slotId] = valueId;
        }

        var valueType = context.Contract.Type.GenericTypeArguments[0];
        context.Result = CreateExternalValue(valueType, valueId, valuesStorage);

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
    private object CreateExternalValue(Type valueType, TValueId valueId, IReadWrite<TValueId, object> storage)
    {
        var idType = typeof(TValueId);
        var readWriteTypeCastedType = typeof(ReadWriteTypeCasted<,>).MakeGenericType(idType, valueType);
        var externalValueType = typeof(ExternalValue<,>).MakeGenericType(idType, valueType);

        var readWriteTypeCasted = Activator.CreateInstance(readWriteTypeCastedType, storage)!;
        return Activator.CreateInstance(externalValueType, valueId, readWriteTypeCasted)!;
    }
}