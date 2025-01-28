using System;

namespace GMutagen.v9.Resolving.Nodes.Internal.Resolvers;

public class SemanticNameAttribute(string name) : Attribute
{
    public string Name { get; set; } = name;
}