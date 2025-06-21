using GMutagen.v9.IO.Interfaces;
using GMutagen.v9.Logging.Messages.Realizations;
using GMutagen.v9.Resolving.Contexts;
using GMutagen.v9.Resolving.Contexts.Key;
using GMutagen.v9.Resolving.Nodes.Decorator;
using GMutagen.v9.Resolving.Nodes.Interfaces;
using GMutagen.v9.Resolving.Nodes.MetaData.Models;
using GMutagen.v9.Resolving.Nodes.MetaData.Models.Interfaces;
using GMutagen.v9.Values.Interfaces;
using Logger.Logger.Common;
using Logger.Logger.Interfaces;

namespace GMutagen.v9.Resolving.Nodes.MetaData.Creation;

public class CreateValueMetaData<TId>(
    IResolverNode resolver,
    IReadWrite<TId, ValueMetaData<TId>> readWrite,
    ILogger<Global> logger)
    : RecursiveResolverNode(resolver)
    where TId : notnull
{
    public override bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IValue)))
        {
            logger.LogWarning(new IsNotAssignable(context.Type, typeof(IValue), this));
            return false;
        }

        var keyType = KeyType.Id;
        var success = context.TryGetKey<TId>(keyType, out var id);
        if (success is false)
        {
            logger.LogWarning(new KeysDoNotContains(context.Keys, keyType, this));
            return false;
        }

        var valueType = context.Type.GenericTypeArguments[0];
        var meta = new ValueMetaData<TId>(id, valueType);

        var contractMetaDataContext = new Context(typeof(IContractMetaData), parentContext: context);
        if (!Resolver.Resolve(contractMetaDataContext))
        {
            logger.LogWarning(new CanNotResolveMetaData(contractMetaDataContext, typeof(IContractMetaData), this));
            return false;
        }

        var contractMetaData = (IContractMetaData)contractMetaDataContext.Instance!;
        contractMetaData.Store(context, meta);

        readWrite.Write(id, meta);
        Cache(context, meta);
        
        logger.LogInfo(new MetaDataCreated<ValueMetaData<TId>>(context, meta, this));

        return false;
    }

    private void Cache(Context parentContext, object result)
    {
        parentContext.Cache[typeof(IValueMetaData)] = result;
    }
}