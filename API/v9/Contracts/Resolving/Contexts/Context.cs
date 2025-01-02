using System;
using System.Collections.Generic;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Contracts.Resolving.Contexts.Option;

namespace GMutagen.v9.Contracts.Resolving.Contexts;

public class Context
{
    public readonly Context? ParentContext;
    public readonly Dictionary<object, object> Cache;

    public readonly Type Type;
    public readonly Keys? Keys;
    public readonly Options? Options;

    public object? Instance;

    public Context()
    {
        Cache = new Dictionary<object, object>();
        Keys = null;
        Options = null;
    }

    public Context(Type type, Keys? keys = null, Options? options = null, Context? parentContext = null)
    {
        Cache = new Dictionary<object, object>();
        Type = type;
        Options = options;
        ParentContext = parentContext;
        Keys = keys;
    }

    public bool TryGetOption<T>(OptionType optionType, out T option)
    {
        option = default;
        
        if (Options == null)
            return false;

        var success = Options.TryGetOption(optionType, out option);
        return success;
    }
    
    public bool TryGetKey<T>(KeyType keyType, out T key)
    {
        key = default;
        
        if (Keys == null)
            return false;

        var success = Keys.TryGetKey(keyType, out key);
        return success;
    }

    public static Context From(Context context, Type? type = null, Keys? keys = null, Options? options = null, Context? parentContext = null)
    {
        var newType = type ?? context.Type;
        var newKeys = keys ?? context.Keys;
        var newOptions = options ?? context.Options;
        var newParent = parentContext ?? context.ParentContext;
        
        var newContext = new Context(newType, newKeys, newOptions, newParent);
        return newContext;
    }
}