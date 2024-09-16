using System.Collections.Generic;

namespace GMutagen.v8.Contracts;

public class ContractValue<TSlotId, TValueId> : Dictionary<TSlotId, TValueId>
    where TSlotId : notnull { }