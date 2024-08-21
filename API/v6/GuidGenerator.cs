using System;

namespace GMutagen.v6;

public class GuidGenerator : IGenerator<Guid>
{
    public Guid Generate() => Guid.NewGuid();
}