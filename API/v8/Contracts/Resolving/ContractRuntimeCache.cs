using System.Collections.Generic;

namespace GMutagen.v8.Contracts.Resolving;

public class ContractRuntimeCache<TContractId>
    where TContractId : notnull
{
    private readonly Dictionary<TContractId, object> _idToImplementation = new();
    private readonly Dictionary<object, TContractId> _implementationToId = new();

    public void Add(TContractId id, object implementation)
    {
        _idToImplementation.Add(id, implementation);
        _implementationToId.Add(implementation, id);
    }

    public object GetById(TContractId id) => _idToImplementation[id];
    public TContractId GetByImplementation(object implementation) => _implementationToId[implementation];

    public bool ContainsById(TContractId id) => _idToImplementation.ContainsKey(id);
    public bool ContainsByImplementation(object implementation) => _implementationToId.ContainsKey(implementation);

    public bool TryGetById(TContractId id, out object implementation) => _idToImplementation.TryGetValue(id, out implementation!);
    public bool TryGetByImplementation(object implementation, out TContractId id) => _implementationToId.TryGetValue(implementation, out id!);

    public void RemoveById(TContractId id)
    {
        var implementation = _idToImplementation[id];

        Remove(id, implementation);
    }
    public void RemoveById(object implementation)
    {
        var id = _implementationToId[implementation];

        Remove(id, implementation);
    }

    private void Remove(TContractId id, object implementation)
    {
        _idToImplementation.Remove(id);
        _implementationToId.Remove(implementation);
    }
}