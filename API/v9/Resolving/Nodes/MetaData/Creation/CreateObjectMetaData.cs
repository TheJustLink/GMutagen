using GMutagen.v9.IO.Interfaces;
using GMutagen.v9.Logging.Logger.Common;
using GMutagen.v9.Logging.Logger.Interfaces;
using GMutagen.v9.Logging.Messages.Realizations;
using GMutagen.v9.Objects.Interfaces;
using GMutagen.v9.Resolving.Contexts;
using GMutagen.v9.Resolving.Contexts.Key;
using GMutagen.v9.Resolving.Nodes.Interfaces;
using GMutagen.v9.Resolving.Nodes.MetaData.Models;
using GMutagen.v9.Resolving.Nodes.MetaData.Models.Interfaces;

namespace GMutagen.v9.Resolving.Nodes.MetaData.Creation;

public class CreateObjectMetaData<TId, TContractId>(
    IReadWrite<TId, ObjectMetaData<TContractId>> readWrite, ILogger<Global> logger) : IResolverNode where TId : notnull
{
    public bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IObject)))
        {
            logger.LogWarning(new IsNotAssignable(context.Type, typeof(IObject), this));
            return false;
        }

        var keyType = KeyType.Id;
        var success = context.TryGetKey<TId>(keyType, out var id);
        if (success is false)
        {
            logger.LogWarning(new KeysDoNotContains(context.Keys, keyType, this));
            return false;
        }
        
        var meta = new ObjectMetaData<TContractId>();
        
        readWrite.Write(id, meta);
        Cache(context, meta);

        logger.LogInfo(new MetaDataCreated<ObjectMetaData<TContractId>>(context, meta, this));
        
        return false;
    }
    
    private void Cache(Context parentContext, object result)
    {
        parentContext.Cache[typeof(IObjectMetaData)] = result;
    }
}