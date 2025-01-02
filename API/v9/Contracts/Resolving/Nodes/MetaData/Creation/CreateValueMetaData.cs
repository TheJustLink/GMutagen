using GMutagen.v9.IO;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Contracts.Resolving.Nodes.From;
using GMutagen.v9.Values;

namespace GMutagen.v9.Contracts.Resolving.Nodes.MetaData.Creation;

public class CreateValueMetaData<TId>(
    IResolverNode resolver,
    IReadWrite<TId, ValueMetaData<TId>> readWrite)
    : RecursiveResolverNode(resolver)
    where TId : notnull
{
    public override bool Resolve(Context context)
    {
        if (typeof(IValue).IsAssignableFrom(context.Type))
            return false;
        
        var success = context.TryGetKey<TId>(KeyType.Id, out var id);
        if (success is false)
            return false;
        
        var meta = new ValueMetaData<TId>(id);

        var contractMetaDataContext = new Context(typeof(IContractMetaData), parentContext: context);
        if (!Resolver.Resolve(contractMetaDataContext))
            return false;

        var contractMetaData = (IContractMetaData)contractMetaDataContext.Instance!;
        contractMetaData.Store(context, meta);
        
        readWrite.Write(id, meta);
        
        return Resolver.Resolve(context);
    }
}