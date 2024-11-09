using System;
using System.Collections.Generic;
using GMutagen.v8.Contracts;

namespace GMutagen.v9.Objects;

public interface IObjectFactory<TId>
{
    IObject<TId> Create(Dictionary<Type, ContractDescriptor> contracts);
    IObject<TId> Create(Dictionary<Type, ContractDescriptor> contracts, TId id);
}