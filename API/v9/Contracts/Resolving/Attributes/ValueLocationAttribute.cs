using System;
using GMutagen.v9.Contracts.Resolving.Nodes.Internal.Enums;

namespace GMutagen.v9.Contracts.Resolving.Attributes;

public class ValueLocationAttribute : Attribute
{
    public LocationType LocationType { get; protected set; }
}