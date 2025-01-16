using System;
using System.Collections.Generic;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Option;
using GMutagen.v9.Contracts.Resolving.Nodes.Interfaces;
using GMutagen.v9.Contracts.Resolving.Nodes.Internal.Enums;
using GMutagen.v9.IO.Factories;
using GMutagen.v9.IO.Interfaces;
using GMutagen.v9.IO.Sources.Dictionary;
using GMutagen.v9.Logging.Logger.Common;
using GMutagen.v9.Logging.Logger.Interfaces;
using GMutagen.v9.Logging.Messages;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Internal.Resolvers;

public class StorageResolver(ILogger<Global> logger) : IResolverNode
{
    public bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IReadWrite)))
        {
            logger.LogWarning(new IsNotAssignable(context.Type, typeof(IReadWrite), this));
            return false;
        }

        var optionType = OptionType.Location;
        var success = context.TryGetOption<LocationType>(optionType, out var location);
        if (success is false)
        {
            logger.LogInfo(new OptionsDoNotContains(context.Options, optionType, this));
            location = LocationType.Memory;
            logger.LogInfo(new Fallback<LocationType>(context.Options, location, this));
        }

        var successResolution = false;

        switch (location)
        {
            case LocationType.Memory:
                successResolution = ResolveFromMemory(context);
                break;
        }

        return successResolution;
    }

    private bool ResolveFromMemory(Context context)
    {
        var iReadWriteType = context.Type;
        var idType = iReadWriteType.GenericTypeArguments[0];
        var valueType = iReadWriteType.GenericTypeArguments[1];

        var dictionaryType = typeof(Dictionary<,>).MakeGenericType(idType, valueType);
        var writeType = typeof(DictionaryWrite<,>).MakeGenericType(idType, valueType);
        var readType = typeof(DictionaryRead<,>).MakeGenericType(idType, valueType);
        var readWriteType = typeof(ReadWrite<,>).MakeGenericType(idType, valueType);


        var dictionary = Activator.CreateInstance(dictionaryType);
        var write = Activator.CreateInstance(writeType, dictionary);
        var read = Activator.CreateInstance(readType, dictionary);
        var readWrite = Activator.CreateInstance(readWriteType, read, write);


        context.Instance = readWrite;
        logger.LogInfo(new SuccessfullyResolved(context, this));
        return true;
    }
}