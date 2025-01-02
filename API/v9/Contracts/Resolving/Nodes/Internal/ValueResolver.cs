using System;
using GMutagen.v9.IO;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Contracts.Resolving.Nodes.From;
using GMutagen.v9.Values;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Internal;

public class ValueResolver<TValueId>(IResolverNode resolver) : RecursiveResolverNode(resolver) where TValueId : notnull
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
        var valueType = context.Type.GenericTypeArguments[0];

        var success = TryResolveStorage(context, valueType, out var storage);
        if (success is false)
            return false;

        success = TryCreateExternalValue(storage!, valueType, context);
        
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

    private bool TryCreateExternalValue(object storage, Type valueType, Context context)
    {
        var success = context.TryGetKey<TValueId>(KeyType.Id, out var valueId);
        if (success is false)
            success = context.TryGetKey<TValueId>(KeyType.Index, out valueId);

        if (success is false)
            return false;
        
        var valueFactory = CreateValueFactory(valueType);
        context.Instance = valueFactory.Create(valueId, storage);
        return true;
    }

    private IValueFactory CreateValueFactory(Type valueType)
    {
        var factoryType = typeof(ExternalValueFactory<>).MakeGenericType(typeof(TValueId), valueType);
        return (Activator.CreateInstance(factoryType) as IValueFactory)!;
    }

    private interface IValueFactory
    {
        object Create(TValueId id, object storage);
    }

    private class ExternalValueFactory<TValueType> : IValueFactory
    {
        public object Create(TValueId id, object storageObj)
        {
            var storage = (IReadWrite<TValueId, TValueType>)storageObj;
            if (storage.Contains(id) is false)
                storage[id] = default!;
            
            return new ExternalValue<TValueId, TValueType>(id, storage);
        }
    }
}