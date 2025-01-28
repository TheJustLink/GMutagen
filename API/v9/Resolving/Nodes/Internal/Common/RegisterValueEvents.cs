using System;
using System.Collections.Generic;
using System.Reflection;
using GMutagen.v9.Events;
using GMutagen.v9.Extensions;
using GMutagen.v9.Resolving.Contexts;
using GMutagen.v9.Resolving.Contexts.Option;
using GMutagen.v9.Resolving.Nodes.Decorator;
using GMutagen.v9.Resolving.Nodes.Interfaces;
using GMutagen.v9.Resolving.Nodes.Internal.Resolvers;
using GMutagen.Values;

namespace GMutagen.v9.Resolving.Nodes.Internal.Common;

public class RegisterValueEvents<TId>(IResolverNode resolver, IConfiguration<string, string> semanticNames)
    : RecursiveResolverNode(resolver) where TId : notnull
{
    public override bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IValue)))
            return false;

        if (!context.TryGetOption(OptionType.Info, out ParameterInfo parameterInfo))
            return false;

        var attributes = parameterInfo.GetCustomAttributes();
        if (!attributes.Contains<GenerateValueEventsAttribute>())
            return false;

        if (context.Type == typeof(ExternalValue<,>))
            RegisterAsExternalValue(context);
        
        if (TryGetSemanticName(context, attributes, out var name))
            RegisterWithSemanticName(name);
        
        RegisterWithClassName(context);

        return false;
    }

    private void RegisterAsExternalValue(Context context)
    {
        var type = typeof(ValueEventsStorage<TId>);

        var valueEventStorageContext = new Context(type);
        var success = Resolver.Resolve(valueEventStorageContext);
        if (!success)
            return;

        var value = (IValue<TId>)context.Instance!;
        var valueEventStorage = (ValueEventsStorage<TId>)valueEventStorageContext.Instance!;
        if (valueEventStorage.Contains(value))
            return;

        valueEventStorage.Write(value, new ValueEvents<TId>(value));
    }

    private void RegisterWithSemanticName(string name)
    {
        var objectValueStorageContext = new Context(typeof(ObjectValueStorage));
        var result = Resolver.Resolve(objectValueStorageContext);
        if (!result)
            return;

        var objectValueStorage = (ObjectValueStorage)objectValueStorageContext.Instance!;
        if (objectValueStorage.Contains(name))
            return;

        var type = typeof(ValueEventsStorage<TId>);

        var valueEventStorageContext = new Context(type);
        var success = Resolver.Resolve(valueEventStorageContext);
        if (!success)
            return;
        
        var value = (IValue<TId>)objectValueStorage.Read(name);
        
        var valueEventStorage = (ValueEventsStorage<TId>)valueEventStorageContext.Instance!;
        if (valueEventStorage.Contains(value))
            return;

        valueEventStorage.Write(value, new ValueEvents<TId>(value));
    }

    private void RegisterWithClassName(Context context)
    {
        var name = string.Join(".", context.ParentContext!.Type.Name, context.Type.Name);
        var type = typeof(ValueEventsStorage<TId>);

        var valueEventStorageContext = new Context(type);
        var success = Resolver.Resolve(valueEventStorageContext);
        if (!success)
            return;

        var value = (IValue<TId>)context.Instance!;

        var valueEventStorage = (ValueEventsStorage<TId>)valueEventStorageContext.Instance!;
        if (valueEventStorage.Contains(value))
            return;

        valueEventStorage.Write(value, new ValueEvents<TId>(value));
    }

    private bool TryGetSemanticName(Context context, IEnumerable<Attribute> attributes, out string name)
    {
        if (TryGetSemanticNameFromInfo(attributes, out name))
            return true;

        if (TryGetSemanticNameFromConfiguration(context, out name))
            return true;

        return false;
    }

    private bool TryGetSemanticNameFromConfiguration(Context context, out string name)
    {
        var id = string.Join(".", context.ParentContext!.Type.Name, context.Type.Name);
        if (semanticNames.TryGetValue(id, out name))
            return true;

        return false;
    }

    private bool TryGetSemanticNameFromInfo(IEnumerable<Attribute> attributes, out string name)
    {
        name = String.Empty;
        if (attributes.Contains<SemanticNameAttribute>())
        {
            var semanticNameAttribute = attributes.Get<SemanticNameAttribute>();
            name = semanticNameAttribute!.Name;
        }

        return true;
    }
}