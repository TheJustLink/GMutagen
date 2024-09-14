using System.Collections.Generic;

namespace GMutagen.v8.Test;

public class ObjectTemplateBuilder
{
    private readonly HashSet<ContractDescriptor> _contracts = new();

    public ObjectTemplate Build() => new(_contracts);

    public ObjectTemplateBuilder Add<TContract, TImplementation>()
        where TContract : class where TImplementation : TContract
    {
        return Add(ContractDescriptor.Create<TContract, TImplementation>());
    }
    public ObjectTemplateBuilder Add<TContract>() where TContract : class
    {
        return Add(ContractDescriptor.Create<TContract>());
    }
    public ObjectTemplateBuilder Add<TContract>(TContract implementation) where TContract : class
    {
        return Add(ContractDescriptor.Create<TContract>(implementation));
    }
    public ObjectTemplateBuilder Add(ContractDescriptor contract)
    {
        _contracts.Add(contract);

        return this;
    }
}