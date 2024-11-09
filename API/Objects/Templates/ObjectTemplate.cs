using System.Collections.Generic;
using GMutagen.Contracts;

namespace GMutagen.Objects.Templates;

public class ObjectTemplate
{
    private readonly HashSet<ContractDescriptor> _contracts;

    public ObjectTemplate(HashSet<ContractDescriptor> contracts)
    {
        _contracts = contracts;
    }

    public IEnumerable<ContractDescriptor> Contracts => _contracts;
}