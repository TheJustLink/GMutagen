using System;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Contracts.Resolving.Nodes.From;
using GMutagen.v9.Contracts.Resolving.Nodes.Internal;
using GMutagen.v9.IO;
using GMutagen.v9.Values;

namespace GMutagen.v9.Contracts.Resolving.Nodes.MetaData;

public class ValueFromMetaCache<TSlotId, TValueId>(IResolverNode resolver, KeyType idKeyType, IReadWrite<TSlotId, ValueMetaData<TValueId>> readWrite) : RecursiveResolverNode(resolver)
{
    public override bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IValue)))
            return false;

        var success = ResolveValue(context);
        return success;
    }

    private bool ResolveValue(Context context)
    {
        if (!context.TryGetKey(idKeyType, out TSlotId slotId))
            return false;

        if (!readWrite.Contains(slotId))
            return false;
        
        var metaData = readWrite.Read(slotId);
        
        var valueType = context.Type.GenericTypeArguments[0];

        var success = TryResolveStorage(context, valueType, out var storage);
        if (success is false)
            return false;

        success = TryCreateExternalValue(storage!, valueType, context, metaData.Id);
        
        if (success is false)
            return false;

        return true;
    }

    private object TryResolveStorage(Context context, Type valueType, out object? storage)
    {
        storage = null;
        var type = typeof(IReadWrite<,>).MakeGenericType(typeof(TValueId), valueType);
        var storageContext = new Context(type, options: context.Options, parentContext: context);

        var success = Resolver.Resolve(storageContext);
        if(success is false)
            return false;

        storage = storageContext.Instance;
        return true;
    }

    private bool TryCreateExternalValue(object storage, Type valueType, Context context, TValueId id)
    {
        var valueFactory = CreateValueFactory(valueType);
        context.Instance = valueFactory.Create(id, storage);
        return true;
    }

    private IValueFactory<TValueId> CreateValueFactory(Type valueType)
    {
        var factoryType = typeof(ExternalValueFactory<,>).MakeGenericType(typeof(TValueId), valueType);
        return (Activator.CreateInstance(factoryType) as IValueFactory<TValueId>)!;
    }
}