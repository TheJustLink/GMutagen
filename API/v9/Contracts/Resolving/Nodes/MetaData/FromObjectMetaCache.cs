using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Objects;

namespace GMutagen.v9.Contracts.Resolving.Nodes.MetaData;

public class FromObjectMetaCache : IResolverNode
{
    public bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IObject)))
            return false;

        return false;
    }
}