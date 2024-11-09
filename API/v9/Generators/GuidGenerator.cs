using System;

namespace GMutagen.v9.Generators;

public class GuidGenerator : IGenerator<Guid>
{
    public Guid Generate() => Guid.NewGuid();
}