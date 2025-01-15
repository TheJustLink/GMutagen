using System;

namespace GMutagen.v9.Contracts.Resolving.Nodes.MetaData;

public class ValueMetaData<TId> : IValueMetaData
{
    public TId Id { get; private set; }

    public Type ValueType { get; private set; }

    public ValueMetaData(TId id, Type valueType)
    {
        Id = id;
        ValueType = valueType;
    }
}