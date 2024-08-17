using System;

namespace GMutagen.v5.Container;

public abstract class ContainerContext
{
    public Type KeyType { get; set; }

    protected readonly ObjectTemplateContainer Container;

    protected ContainerContext(ObjectTemplateContainer container)
    {
        Container = container;
    }
}