using GMutagen.v9.Generators;
using GMutagen.v9.IO;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Objects;

namespace GMutagen.v9.Contracts.Resolving.Nodes.MetaData.Creation;

public class CreateObjectMetaData<TId, TContractId>(
    IReadWrite<TId, ObjectMetaData<TContractId>> readWrite) : IResolverNode where TId : notnull
{
    public bool Resolve(Context context)
    {
        if (typeof(IObject).IsAssignableFrom(context.Type))
            return false;
        
        var success = context.TryGetKey<TId>(KeyType.Id, out var id);
        if (success is false)
            return false;
        
        var meta = new ObjectMetaData<TContractId>();
        
        readWrite.Write(id, meta);

        return false;
    }
}