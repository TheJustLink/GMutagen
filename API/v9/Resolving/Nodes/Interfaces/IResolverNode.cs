using GMutagen.v9.Resolving.Contexts;

namespace GMutagen.v9.Resolving.Nodes.Interfaces;

public interface IResolverNode
{
    bool Resolve(Context context);
}