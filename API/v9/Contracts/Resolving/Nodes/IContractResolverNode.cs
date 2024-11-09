using System;

namespace GMutagen.v9.Contracts.Resolving.Nodes;

public interface IContractResolverNode
{
    bool Resolve(Context context);
}