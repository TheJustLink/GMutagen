using System;
using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Contracts.Resolving.Nodes.MetaData.Models.Interfaces;

public interface IContractMetaData
{
    public Type Type { get; }

    void Store(Context context, object valueMetaData);
}