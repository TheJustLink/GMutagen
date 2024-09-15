using System;
using System.Collections.Generic;

using GMutagen.v8.Contracts;

namespace GMutagen.v8.Objects;

public interface IObjectFactory
{
    IObject Create(Dictionary<Type, ContractDescriptor> contracts);
}