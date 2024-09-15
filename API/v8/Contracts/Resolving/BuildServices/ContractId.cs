using System;

namespace GMutagen.v8.Contracts.Resolving.BuildServices;

class ContractId : Id
{
    public ContractId(Type type, object value) : base(type, value) { }
}