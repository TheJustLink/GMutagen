using System;
using System.Collections.Generic;

namespace GMutagen.v8.Objects;

public class ObjectValue<TContractId>
{
    public readonly Dictionary<Type, TContractId> Contracts;
    public ObjectValue() : this(new Dictionary<Type, TContractId>()) { }
    public ObjectValue(Dictionary<Type, TContractId> contracts) => Contracts = contracts;
}