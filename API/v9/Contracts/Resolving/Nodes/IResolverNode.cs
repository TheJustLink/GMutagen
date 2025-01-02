using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Contracts.Resolving.Nodes;

public interface IResolverNode
{
    bool Resolve(Context context);
}