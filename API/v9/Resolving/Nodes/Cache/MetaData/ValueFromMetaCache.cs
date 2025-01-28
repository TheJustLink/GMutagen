using System;
using GMutagen.v9.IO.Interfaces;
using GMutagen.v9.Logging.Logger.Common;
using GMutagen.v9.Logging.Logger.Interfaces;
using GMutagen.v9.Logging.Messages.Realizations;
using GMutagen.v9.Resolving.Contexts.Key;
using GMutagen.v9.Resolving.Nodes.Decorator;
using GMutagen.v9.Resolving.Nodes.Interfaces;
using GMutagen.v9.Resolving.Nodes.Internal.Factories;
using GMutagen.v9.Resolving.Nodes.Internal.Factories.Interfaces;
using GMutagen.v9.Resolving.Nodes.MetaData.Models;
using GMutagen.v9.Values.Interfaces;

namespace GMutagen.v9.Resolving.Nodes.Cache.MetaData;

public class ValueFromMetaCache<TSlotId, TValueId>(
    IResolverNode resolver,
    KeyType idKeyType,
    IReadWrite<TSlotId, ValueMetaData<TValueId>> readWrite,
    ILogger<Global> logger) : RecursiveResolverNode(resolver)
{
    public override bool Resolve(Contexts.Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IValue)))
        {
            logger.LogWarning(new IsNotAssignable(context.Type, typeof(IValue), this));
            return false;
        }

        var success = ResolveValue(context);
        return success;
    }

    private bool ResolveValue(Contexts.Context context)
    {
        if (!context.TryGetKey(idKeyType, out TSlotId slotId))
        {
            logger.LogWarning(new KeysDoNotContains(context.Keys, idKeyType, this));
            return false;
        }

        if (!readWrite.Contains(slotId))
        {
            logger.LogWarning(
                new ReadWriteDoNotContains<IReadWrite<TSlotId, ValueMetaData<TValueId>>>(readWrite,
                    slotId, this));
            return false;
        }
        
        var metaData = readWrite.Read(slotId);
        
        var valueType = context.Type.GenericTypeArguments[0];

        var success = TryResolveStorage(context, valueType, out var storage);
        if (success is false)
        {
            logger.LogWarning(new CanNotResolveStorage(context, this));
            return false;
        }

        success = TryCreateExternalValue(storage!, valueType, context, metaData.Id);
        
        if (success)
        {
            logger.LogInfo(new SuccessfullyResolved(context, this));
            return true;
        }
        else
        {
            logger.LogInfo(new FailedToResolve(context, this));
            return false;
        }
    }

    private bool TryResolveStorage(Contexts.Context context, Type valueType, out object? storage)
    {
        storage = null;
        var type = typeof(IReadWrite<,>).MakeGenericType(typeof(TValueId), valueType);
        var storageContext = new Contexts.Context(type, options: context.Options, parentContext: context);

        var success = Resolver.Resolve(storageContext);
        if(success is false)
            return false;

        storage = storageContext.Instance;
        return true;
    }

    private bool TryCreateExternalValue(object storage, Type valueType, Contexts.Context context, TValueId id)
    {
        var valueFactory = CreateValueFactory(valueType);
        context.Instance = valueFactory.Create(id, storage);
        return true;
    }

    private IValueFactory<TValueId> CreateValueFactory(Type valueType)
    {
        var factoryType = typeof(ExternalValueFactory<,>).MakeGenericType(typeof(TValueId), valueType);
        return (Activator.CreateInstance(factoryType, logger) as IValueFactory<TValueId>)!;
    }
}