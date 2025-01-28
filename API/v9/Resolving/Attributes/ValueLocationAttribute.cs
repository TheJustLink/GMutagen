using System;
using GMutagen.v9.Resolving.Nodes.Internal.Enums;

namespace GMutagen.v9.Resolving.Attributes;

public class ValueLocationAttribute : Attribute
{
    public LocationType LocationType { get; protected set; }
}