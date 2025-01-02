using GMutagen.v9.IO;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Contracts.Resolving.Nodes.From;

namespace GMutagen.v9.Contracts.Resolving.Nodes.MetaData.Creation;

public class CreateContractMetaData<TId, TValueId>(
    IResolverNode resolver,
    IReadWrite<TId, ContractMetaData<TId, TValueId>> readWrite)
    : RecursiveResolverNode(resolver), IResolverNode where TId : notnull
{
    public override bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IContract)))
            return false;

        var success = context.TryGetKey<TId>(KeyType.Id, out var id);
        if (success is false)
            return false;
        
        var meta = new ContractMetaData<TId, TValueId>(context.Type);

        var contractMetaDataContext = new Context(typeof(IObjectMetaData), parentContext: context);
        if (!Resolver.Resolve(contractMetaDataContext))
            return false;

        var objectMetaData = (IObjectMetaData)contractMetaDataContext.Instance!;
        objectMetaData.Store(context, meta);

        readWrite.Write(id, meta);

        return Resolver.Resolve(context);
    }
}