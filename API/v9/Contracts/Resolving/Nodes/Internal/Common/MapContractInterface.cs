using System;
using System.Collections.Generic;
using GMutagen.v9.Contracts.Descriptors;
using GMutagen.v9.Contracts.Interfaces;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Option;
using GMutagen.v9.Contracts.Resolving.Nodes.Decorator;
using GMutagen.v9.Contracts.Resolving.Nodes.Interfaces;
using GMutagen.v9.Logging.Logger.Common;
using GMutagen.v9.Logging.Logger.Interfaces;
using GMutagen.v9.Logging.Messages;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Internal.Common;

public class MapContractInterface(IResolverNode resolver, ILogger<Global> logger) : RecursiveResolverNode(resolver)
{
    public override bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IContract)))
        {
            logger.LogWarning(new IsNotAssignable(context.Type, typeof(IContract), this));
            return false;
        }

        var parentContext = context.ParentContext;
        var rootContext = context;

        while (parentContext != null)
        {
            rootContext = parentContext;
            parentContext = parentContext.ParentContext;
        }

        var optionType = OptionType.Contracts;
        var success = rootContext.TryGetOption<Dictionary<Type, ContractDescriptor>>
            (optionType, out var descriptors);

        if (success is false)
        {
            logger.LogWarning(new OptionsDoNotContains(rootContext.Options, optionType, this));
            return false;
        }

        success = descriptors.TryGetValue(context.Type, out var descriptor);
        if (success is false)
        {
            logger.LogInfo(new CanNotMapContract(context, descriptors, this));
            return false;
        }

        var newContext = Context.From(context, descriptor!.ImplementationType);

        success = Resolver.Resolve(newContext);
        context.Instance = newContext.Instance;

        if (success)
        {
            logger.LogInfo(new SuccessfullyResolved(context, this));
        }
        else
        {
            logger.LogInfo(new FailedToResolve(context, this));
        }

        return success;
    }
}