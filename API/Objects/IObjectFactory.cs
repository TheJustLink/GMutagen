using System;
using System.Collections.Generic;
using GMutagen.Contracts;

namespace GMutagen.Objects;

public interface IObjectFactory<TId>
{
    IObject<TId> Create(Dictionary<Type, ContractDescriptor> contracts);
    IObject<TId> Create(Dictionary<Type, ContractDescriptor> contracts, TId id);
}