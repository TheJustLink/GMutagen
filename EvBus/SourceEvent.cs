namespace EventBus;

public class SourceEvent : Event
{
    public Identity.Compose.Identity Source { get; set; }
}