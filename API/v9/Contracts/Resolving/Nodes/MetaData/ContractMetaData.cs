using System;
using System.Collections.Generic;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;

namespace GMutagen.v9.Contracts.Resolving.Nodes.MetaData;

public class ContractMetaData<TSlotId, TValueId> : IContractMetaData
    where TSlotId : notnull
{
    public Type Type { get; set; }
    public Dictionary<TSlotId, TValueId> Slots { get; set; }

    public ContractMetaData(Type type) : this(type, new())
    {
    }
    public ContractMetaData(Type type, Dictionary<TSlotId, TValueId> slots)
    {
        Slots = slots;
        Type = type;
    }

    public void Store(Context context, object valueMetaDataObj)
    {
        var valueMetaData = (ValueMetaData<TValueId>)valueMetaDataObj;
        var valueId = valueMetaData.Id;

        var keys = context.Keys;
        var slotId = (TSlotId)keys![KeyType.Index]!;
        
        Slots[slotId] = valueId;
    }
}