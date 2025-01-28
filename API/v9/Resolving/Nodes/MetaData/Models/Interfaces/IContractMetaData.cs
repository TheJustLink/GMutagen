using System;
using GMutagen.v9.Resolving.Contexts;

namespace GMutagen.v9.Resolving.Nodes.MetaData.Models.Interfaces;

public interface IContractMetaData
{
    public Type Type { get; }

    void Store(Context context, object valueMetaData);
}