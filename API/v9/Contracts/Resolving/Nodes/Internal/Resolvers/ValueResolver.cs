using System;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Contracts.Resolving.Nodes.Decorator;
using GMutagen.v9.Contracts.Resolving.Nodes.Interfaces;
using GMutagen.v9.Contracts.Resolving.Nodes.Internal.Factories;
using GMutagen.v9.Contracts.Resolving.Nodes.Internal.Factories.Interfaces;
using GMutagen.v9.IO.Interfaces;
using GMutagen.v9.Logging.Logger.Common;
using GMutagen.v9.Logging.Logger.Interfaces;
using GMutagen.v9.Logging.Messages;
using GMutagen.v9.Values.Interfaces;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Internal.Resolvers;

public class ValueResolver<TValueId>(IResolverNode resolver, ILogger<Global> logger)
    : RecursiveResolverNode(resolver) where TValueId : notnull
{
    public override bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IValue)))
        {
            logger.LogWarning(new IsNotAssignable(context.Type, typeof(IValue), this));
            return false;
        }

        var success = ResolveValue(context);
        return success;
    }

    private bool ResolveValue(Context context)
    {
        var valueType = context.Type.GenericTypeArguments[0];

        var success = TryResolveStorage(context, valueType, out var storage);
        if (success is false)
        {
            logger.LogWarning(new CanNotResolveStorage(context, this));
            return false;
        }

        success = TryCreateExternalValue(storage!, valueType, context);

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

    private bool TryResolveStorage(Context context, Type valueType, out object? storage)
    {
        storage = null;
        var type = typeof(IReadWrite<,>).MakeGenericType(typeof(TValueId), valueType);
        var storageContext = new Context(type, options: context.Options, parentContext: context);

        var success = Resolver.Resolve(storageContext);
        if (success is false)
            return false;

        storage = storageContext.Instance;
        return true;
    }

    private bool TryCreateExternalValue(object storage, Type valueType, Context context)
    {
        var keyType = KeyType.Id;
        var success = context.TryGetKey<TValueId>(keyType, out var valueId);
        if (success is false)
        {
            logger.LogWarning(new KeysDoNotContains(context.Keys, keyType, this));
            return false;
        }

        var valueFactory = CreateValueFactory(valueType); //TODO: Add caching for factories
        context.Instance = valueFactory.Create(valueId, storage);
        return true;
    }

    private IValueFactory<TValueId> CreateValueFactory(Type valueType)
    {
        var factoryType = typeof(ExternalValueFactory<,>).MakeGenericType(typeof(TValueId), valueType);
        return (Activator.CreateInstance(factoryType, logger) as IValueFactory<TValueId>)!;
    }
}