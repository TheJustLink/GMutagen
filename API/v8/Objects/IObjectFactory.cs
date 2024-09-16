using System;
using System.Collections.Generic;

using GMutagen.v8.Contracts;

namespace GMutagen.v8.Objects;

public interface IObjectFactory<out TId>
{
    IObject<TId> Create(Dictionary<Type, ContractDescriptor> contracts);
}