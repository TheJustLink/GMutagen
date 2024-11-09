using System;
using System.Collections.Generic;

namespace GMutagen.Contracts;

public class ContractValue<TSlotId, TValueId>
    where TSlotId : notnull
{
    public Type Type { get; set; }
    public Dictionary<TSlotId, TValueId> Slots { get; set; }

    public ContractValue() { }
    public ContractValue(Type type)
        : this(type, new()) { }
    public ContractValue(Type type, Dictionary<TSlotId, TValueId> slots)
    {
        Slots = slots;
        Type = type;
    }
}