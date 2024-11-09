using GMutagen.v9.Values;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Internal;

public class IValueResolver : IContractResolverNode
{
    public bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IValue))) 
            return false;
        
        return false;
    }
}