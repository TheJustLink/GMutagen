using System;
using System.Collections.Generic;
using GMutagen.v9.Contracts.Descriptors;
using GMutagen.v9.Contracts.Interfaces;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Contracts.Resolving.Contexts.Option;
using GMutagen.v9.Contracts.Resolving.Nodes.Decorator;
using GMutagen.v9.Contracts.Resolving.Nodes.Interfaces;
using GMutagen.v9.Generators.Interfaces;
using GMutagen.v9.Logging.Logger.Common;
using GMutagen.v9.Logging.Logger.Interfaces;
using GMutagen.v9.Logging.Messages;
using GMutagen.v9.Objects;
using GMutagen.v9.Objects.Interfaces;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Internal.Resolvers;

public class ObjectResolver<TObjectId, TContractId>(
    IResolverNode resolver,
    IGenerator<TContractId> generator,
    ILogger<Global> logger)
    : RecursiveResolverNode(resolver) where TObjectId : notnull
{
    public override bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IObject)))
        {
            logger.LogWarning(new IsNotAssignable(context.Type, typeof(IContract), this));
            return false;
        }

        var keyType = KeyType.Id;
        var successGetKey = context.TryGetKey<TObjectId>(keyType, out var id);

        if (successGetKey is false)
        {
            logger.LogWarning(new KeysDoNotContains(context.Keys, keyType, this));
            return false;
        }

        var optionType = OptionType.Contracts;
        var successGetOption = context.TryGetOption<Dictionary<Type, ContractDescriptor>>(optionType,
            out var contracts);

        if (successGetOption is false)
        {
            logger.LogWarning(new OptionsDoNotContains(context.Options, optionType, this));
            return false;
        }

        var successCreation = TryCreateImplementations(contracts, context, out var implementations);

        if (successCreation is false)
        {
            logger.LogInfo(new CanNotCreateImplementations(context,contracts, implementations, this));
            logger.LogInfo(new FailedToResolve(context, this));
        }

        var obj = new Object<TObjectId>(id, implementations);
        context.Instance = obj;
        
        logger.LogInfo(new SuccessfullyResolved(context, this));
        return true;
    }

    private bool TryCreateImplementations(Dictionary<Type, ContractDescriptor> contracts, Context context,
        out Dictionary<Type, object> implementations)
    {
        implementations = new Dictionary<Type, object>(contracts.Count);

        foreach (var (type, descriptor) in contracts)
        {
            var id = generator.Generate();
            var keys = new Keys(2)
                .Add(KeyType.Id, id)
                .Add(KeyType.DeclaredType, type);

            var contractContext = new Context(descriptor.ImplementationType!, keys, parentContext: context);
            var success = Resolver.Resolve(contractContext);

            if (success)
            {
                implementations[type] = contractContext.Instance!;
                logger.LogInfo(new SuccessfullyResolved(contractContext, this));
            }
            else
            {
                logger.LogInfo(new FailedToResolve(contractContext, this));
                return false;
            }
        }

        return true;
    }
}