using System.Reflection;
using EventBus;
using GMutagen.v9.Events;
using GMutagen.v9.Resolving.Attributes;
using GMutagen.v9.Resolving.Contexts;
using GMutagen.v9.Resolving.Contexts.Option;
using GMutagen.v9.Resolving.Nodes.Decorator;
using GMutagen.v9.Resolving.Nodes.Interfaces;
using GMutagen.v9.Resolving.Nodes.Internal.Common;
using GMutagen.v9.Values.Interfaces;
using Identity.Realizations;

namespace VariableAccessingApi;

public class ValueRegistrationNode(IResolverNode resolver, IConfiguration<string, string> configuration, IValueAddressationScheme adressationScheme) : RecursiveResolverNode(resolver)
{
    public bool CanResolve(Context context)
    {
        return context.Type.IsAssignableTo(typeof(IValue));
    }

    public override bool Resolve(Context context)
    {
        if (!CanResolve(context))
            return Resolver.Resolve(context);
        
        var type = context.Type;
        var value = (IValue)context.Instance!;
        
        var idAttr = type.GetCustomAttribute<IdAttribute>();
        var id = idAttr?.Id;
        
        var shouldWrapWithEvents =
            type.GetCustomAttribute<GenerateValueEventsAttribute>() is not null ||
            type.IsAssignableTo(typeof(IValueWithEvents));
        
        if (shouldWrapWithEvents)
        {
            var valueType = type.GetGenericArguments()[0];
            var wrapperType = typeof(ValueEvents<>).MakeGenericType(valueType);
            value = (IValue)Activator.CreateInstance(wrapperType, value)!;
            context.Instance = value;
        }

        if (!context.TryGetOption<ParameterInfo>(OptionType.Info, out var info))
            return true;


        var name = info.Name;
        var resolvingContract = context.ParentContext!.Type;
        var addressContext = new ValueAddressationContext()
        {
            ObjectWhereDefined = resolvingContract,
            DeclaredName = name,
        };
        
        var address = adressationScheme.Address(addressContext);
        
        if (address == null! && id == null)
            return true; 
        
        var storageContext = new Context(typeof(ObjectValueStorage));
        if (!Resolver.Resolve(storageContext))
            return false;

        var storage = (ObjectValueStorage)storageContext.Instance!;

        var semanticName = configuration[address!];
        var semanticId = new Id<string>(semanticName);
        if (semanticName != null!)
            storage.Write(semanticId, value);

        if (id != null)
            storage.Write(new Id<object>(id), value);
        
        if (value is IValueWithEvents valueWithEvents)
        {
            var tId = id?.GetType() ?? typeof(object);
            var valueEventsStorageType = typeof(ValueEventsStorage).MakeGenericType(tId);
            var valueEventsStorageContext = new Context(valueEventsStorageType);
            if (!Resolver.Resolve(valueEventsStorageContext))
                return false;

            var valueEventsStorage = (ValueEventsStorage)valueEventsStorageContext.Instance!;
            valueEventsStorage.Write(semanticId, valueWithEvents.Events);
        }

        return true;
    }
}