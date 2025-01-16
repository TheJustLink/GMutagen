using System;
using GMutagen.v9.Generators.Interfaces;

namespace GMutagen.v9.Generators;

public class GuidGenerator : IGenerator<Guid>
{
    public Guid Generate() => Guid.NewGuid();
}