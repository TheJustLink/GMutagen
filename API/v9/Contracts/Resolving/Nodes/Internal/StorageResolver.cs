using System;
using System.Collections.Generic;
using GMutagen.v9.IO;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Option;
using GMutagen.v9.IO.Sources.Dictionary;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Internal;

public class StorageResolver : IResolverNode
{
    public bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IReadWrite)))
            return false;
        
        var success = context.TryGetOption<LocationType>(OptionType.Location, out var location);
        if (success is false)
            location = LocationType.Memory;

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
        return true;
    }
}

public enum LocationType
{
    Memory,
    File,
}