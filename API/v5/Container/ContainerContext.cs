using System;
using GMutagen.v5.Container;

namespace GMutagen.v5;

public abstract class ContainerContext
{
    public Type KeyType { get; set; }

    protected readonly ObjectTemplateContainer Container;

    protected ContainerContext(ObjectTemplateContainer container)
    {
        Container = container;
    }
}