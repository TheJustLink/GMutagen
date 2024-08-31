using System;
using GMutagen.v6.Objects;

namespace GMutagen.v6.Id;

public class GuidGenerator : IGenerator<Guid>
{
    public Guid Generate() => Guid.NewGuid();
}