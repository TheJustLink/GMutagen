using System;
using System.Collections.Generic;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Contracts.Resolving.Nodes.MetaData.Models.Interfaces;

namespace GMutagen.v9.Contracts.Resolving.Nodes.MetaData.Models;

public class ObjectMetaData<TContractId> : IObjectMetaData
{
    private readonly Dictionary<Type, TContractId> _contracts = new();

    public IReadOnlyDictionary<Type, TContractId> Contracts => _contracts;

    public void Store(Context context, object contractMetaDataObj)
    {
        var contractMetaData = (IContractMetaData)contractMetaDataObj;
        var contractType = contractMetaData.Type;

        var keys = context.Keys;
        var contractId = (TContractId)keys![KeyType.Id]!;

        _contracts.TryAdd(contractType, contractId);
    }
}