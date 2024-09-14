using System;
using System.Collections.Generic;

namespace GMutagen.v8.Test;

public interface IObjectFactory
{
    IObject Create(Dictionary<Type, ContractDescriptor> contracts);
}