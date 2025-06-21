namespace EventBus;

public interface IEventAddressationScheme
{
    string Address(EventAddressationContext context);
}
public interface IValueAddressationScheme
{
    string Address(ValueAddressationContext context);
}

public class ValueAddressationContext
{
    public Type ObjectWhereDefined { get; set; }
    public string? DeclaredName { get; set; }
}

public class EventAddressationContext
{
    public object? Sender { get; init; }
    public Event Event { get; init; } = default!;
    public string? ObjectSemanticName { get; init; }
    public string? ValueSemanticName { get; init; }
}

public class ClassEventScheme : IEventAddressationScheme
{
    public string Address(EventAddressationContext context)
    {
        return $"{context.Sender?.GetType().Name}"
               + Constants.DELIMITER_CHAR
               + $"{context.Event.GetType().Name}";
    }
}

public class FullClassEventScheme : IEventAddressationScheme
{
    public string Address(EventAddressationContext context)
    {
        return $"{context.Sender?.GetType().FullName}"
               + Constants.DELIMITER_CHAR
               + $"{context.Event.GetType().Name}";
    }
}

public class EventScheme : IEventAddressationScheme
{
    public string Address(EventAddressationContext context)
    {
        return $"{context.Event.GetType().Name}";
    }
}

public class SemanticEventScheme : IEventAddressationScheme
{
    public string Address(EventAddressationContext context)
    {
        return $"{context.ObjectSemanticName}"
               + Constants.DELIMITER_CHAR
               + $"{context.Event.GetType().Name}";
    }
}

