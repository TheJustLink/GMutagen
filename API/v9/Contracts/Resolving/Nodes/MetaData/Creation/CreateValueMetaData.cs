using GMutagen.v9.IO;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Contracts.Resolving.Nodes.From;
using GMutagen.v9.Values;

namespace GMutagen.v9.Contracts.Resolving.Nodes.MetaData.Creation;

public class CreateValueMetaData<TId>(
    IResolverNode resolver,
    IReadWrite<TId, ValueMetaData<TId>> readWrite,
    KeyType keyType)
    : RecursiveResolverNode(resolver)
    where TId : notnull
{
    public override bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IValue)))
            return false;

        var success = context.TryGetKey<TId>(keyType, out var id);
        if (success is false)
            return false;

        var valueType = context.Type.GenericTypeArguments[0];
        var meta = new ValueMetaData<TId>(id, valueType);

        var contractMetaDataContext = new Context(typeof(IContractMetaData), parentContext: context);
        if (!Resolver.Resolve(contractMetaDataContext))
            return false;

        var contractMetaData = (IContractMetaData)contractMetaDataContext.Instance!;
        contractMetaData.Store(context, meta);

        readWrite.Write(id, meta);
        Cache(context, meta);

        return false;
    }

    private void Cache(Context parentContext, object result)
    {
        parentContext.Cache[typeof(IValueMetaData)] = result;
    }
}