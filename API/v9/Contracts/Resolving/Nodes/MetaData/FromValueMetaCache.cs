using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Values;

namespace GMutagen.v9.Contracts.Resolving.Nodes.MetaData;

public class FromValueMetaCache : IResolverNode
{
    public bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IValue)))
            return false;



        return false;
    }
}