using System;
using System.Collections.Generic;
using GMutagen.v9.Contracts.Descriptors;
using GMutagen.v9.Objects.Interfaces;

namespace GMutagen.v9.Objects.Factories.Interfaces;

public interface IObjectFactory<TId>
{
    IObject<TId> Create(Dictionary<Type, ContractDescriptor> contracts);
    IObject<TId> Create(Dictionary<Type, ContractDescriptor> contracts, TId id);
}