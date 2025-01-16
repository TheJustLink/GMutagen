using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Interfaces;

public interface IResolverNode
{
    bool Resolve(Context context);
}