using System;
using System.Collections.Generic;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Contracts.Resolving.Contexts.Option;
using GMutagen.v9.Contracts.Resolving.Nodes.Decorator;
using GMutagen.v9.Contracts.Resolving.Nodes.Interfaces;
using GMutagen.v9.Contracts.Resolving.Nodes.MetaData.Models;
using GMutagen.v9.IO.Interfaces;
using GMutagen.v9.Logging.Logger.Common;
using GMutagen.v9.Logging.Logger.Interfaces;
using GMutagen.v9.Logging.Messages;
using GMutagen.v9.Objects;
using GMutagen.v9.Objects.Interfaces;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Cache.MetaData;

public class ObjectFromMetaCache<TId>(
    IResolverNode resolver,
    KeyType idKeyType,
    IReadWrite<TId, ObjectMetaData<TId>> readWrite,
    ILogger<Global> logger) : RecursiveResolverNode(resolver) where TId : notnull
{
    public override bool Resolve(Contexts.Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IObject)))
        {
            logger.LogWarning(new IsNotAssignable(context.Type, typeof(IObject), this));
            return false;
        }

        if (!context.TryGetKey(idKeyType, out TId id))
        {
            logger.LogWarning(new KeysDoNotContains(context.Keys, idKeyType, this));
            return false;
        }

        if (!readWrite.Contains(id))
        {
            logger.LogWarning(new ReadWriteDoNotContains<IReadWrite<TId, ObjectMetaData<TId>>>(readWrite, id, this));
            return false;
        }

        var metaData = readWrite.Read(id);

        var optionId = OptionType.ContractsReversed;
        var successGetOption = context.TryGetOption<Dictionary<Type, List<Type>>>(optionId,
            out var contractsReversed);

        if (successGetOption is false)
        {
            logger.LogWarning(new OptionsDoNotContains(context.Options, optionId, this));
            return false;
        }

        var successCreation = TryCreateImplementations(metaData, context, contractsReversed, out var implementations);

        if (successCreation is false)
        {
            logger.LogError(new CanNotCreateReversedImplementations(context, contractsReversed, implementations, this));
            return false;
        }

        var obj = new Object<TId>(id, implementations);
        context.Instance = obj;

        logger.LogInfo(new SuccessfullyResolved(context, this));
        return true;
    }

    private bool TryCreateImplementations(ObjectMetaData<TId> metaData, Contexts.Context context,
        Dictionary<Type, List<Type>> contracts,
        out Dictionary<Type, object> implementations)
    {
        implementations = new Dictionary<Type, object>(metaData.Contracts.Count);

        foreach (var (implementationType, id) in metaData.Contracts)
        {
            var keys = new Keys(2)
                .Add(KeyType.Id, id)
                .Add(KeyType.DeclaredType, implementationType);

            var contractContext = new Contexts.Context(implementationType!, keys, parentContext: context);
            var success = Resolver.Resolve(contractContext);


            var types = contracts[implementationType];
            if (success)
            {
                foreach (var type in types)
                {
                    implementations[type] = contractContext.Instance!;
                }
            }
            else
                return false;
        }

        return true;
    }
}