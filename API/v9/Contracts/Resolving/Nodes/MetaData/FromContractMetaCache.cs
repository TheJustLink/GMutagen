using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Contracts.Resolving.Nodes.MetaData;

public class FromContractMetaCache : IResolverNode
{
    public bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IContract)))
            return false;


        return false;
    }
}