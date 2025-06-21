using System;
using System.Linq;
using System.Reflection;
using EventBus;
using GMutagen.v9.Resolving.Contexts;
using GMutagen.v9.Resolving.Contexts.Option;
using GMutagen.v9.Resolving.Nodes.Interfaces;
using Logger.Logger.Interfaces;

namespace GMutagen.v9.Resolving.Nodes.Decorator;

public abstract class RecursiveResolverNode(IResolverNode resolver) : IResolverNode
{
    protected readonly IResolverNode Resolver = resolver;

    public abstract bool Resolve(Context context);
}

public class EventInjectionNode(
    IResolverNode resolver,
    IEventAddressationScheme eventAddressationScheme,
    ILogger<EventInjectionNode> logger)
    : RecursiveResolverNode(resolver)
{
    public override bool Resolve(Context context)
    {
        if (!base.Resolver.Resolve(context))
        {
            logger.LogWarning($"Unable to resolve inner context for {context.Type.Name}");
            return false;
        }

        if (context.Instance is null)
        {
            logger.LogWarning($"Context.Instance is null in {nameof(EventInjectionNode)}");
            return false;
        }

        if (!TryGetEventBus(out var eventBus))
            return false;
        
        var instance = context.Instance;
        var type = instance.GetType();

        var fields = type
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => typeof(Event).IsAssignableFrom(f.FieldType));

        foreach (var field in fields)
        {
            try
            {
                var evt = (Event)Activator.CreateInstance(field.FieldType)!;
                evt.EventBus = eventBus;

                string semanticName = String.Empty;
                if (context.TryGetOption(OptionType.SemanticName, out semanticName) is false)
                    semanticName = null;

                var eventContext = new EventAddressationContext
                {
                    Event = evt,
                    Sender = instance,
                    ObjectSemanticName = semanticName,
                };
                evt.Topic = eventAddressationScheme.Address(eventContext);

                field.SetValue(instance, evt);

                logger.LogDebug(
                    $"Injected event '{evt.GetType().Name}' into field '{field.Name}' with topic '{evt.Topic}'");
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to inject event into field '{field.Name}' of type '{type.Name}'");
                return false;
            }
        }

        return true;
    }

    private bool TryGetEventBus(out EventBus.EventBus eventBus)
    {
        eventBus = null;
        var eventBusContext = new Context(typeof(EventBus.EventBus));
        if (!Resolver.Resolve(eventBusContext))
            return false;
        eventBus = (EventBus.EventBus)eventBusContext.Instance!;
        return true;
    }
}