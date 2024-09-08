using System;

using GMutagen.v8.Objects;

namespace GMutagen.v8.Id;

public class GuidGenerator : IGenerator<Guid>
{
    public Guid Generate() => Guid.NewGuid();
}