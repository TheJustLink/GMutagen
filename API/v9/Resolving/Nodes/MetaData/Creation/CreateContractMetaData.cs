using GMutagen.v9.Contracts.Interfaces;
using GMutagen.v9.IO.Interfaces;
using GMutagen.v9.Logging.Messages.Realizations;
using GMutagen.v9.Resolving.Contexts;
using GMutagen.v9.Resolving.Contexts.Key;
using GMutagen.v9.Resolving.Nodes.Decorator;
using GMutagen.v9.Resolving.Nodes.Interfaces;
using GMutagen.v9.Resolving.Nodes.MetaData.Models;
using GMutagen.v9.Resolving.Nodes.MetaData.Models.Interfaces;
using Logger.Logger.Common;
using Logger.Logger.Interfaces;

namespace GMutagen.v9.Resolving.Nodes.MetaData.Creation;

public class CreateContractMetaData<TId, TValueId>(
    IResolverNode resolver,
    IReadWrite<TId, ContractMetaData<TId, TValueId>> readWrite,
    ILogger<Global> logger)
    : RecursiveResolverNode(resolver), IResolverNode where TId : notnull
{
    public override bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IContract)))
        {
            logger.LogWarning(new IsNotAssignable(context.Type, typeof(IContract), this));
            return false;
        }

        var keyType = KeyType.Id;
        var success = context.TryGetKey<TId>(keyType, out var id);
        if (success is false)
        {
            logger.LogWarning(new KeysDoNotContains(context.Keys, keyType, this));
            return false;
        }

        var meta = new ContractMetaData<TId, TValueId>(context.Type);

        var objectMetaDataContext = new Context(typeof(IObjectMetaData), parentContext: context);
        if (!Resolver.Resolve(objectMetaDataContext))
        {
            logger.LogWarning(new CanNotResolveMetaData(objectMetaDataContext, typeof(IObjectMetaData), this));
            return false;
        }

        var objectMetaData = (IObjectMetaData)objectMetaDataContext.Instance!;
        objectMetaData.Store(context, meta);

        readWrite.Write(id, meta);
        Cache(context, meta);

        logger.LogInfo(new MetaDataCreated<ContractMetaData<TId, TValueId>>(context, meta, this));

        return false;
    }

    private void Cache(Context parentContext, object result)
    {
        parentContext.Cache[typeof(IContractMetaData)] = result;
    }
}