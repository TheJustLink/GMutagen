using System.Collections.Generic;

namespace GMutagen.v8.Contracts;

public class ContractValue<TSlotId, TValueId> where TSlotId : notnull
{
    public readonly Dictionary<TSlotId, TValueId> Slots;
    public ContractValue() : this(new Dictionary<TSlotId, TValueId>()) { }
    public ContractValue(Dictionary<TSlotId, TValueId> slots) => Slots = slots;
}