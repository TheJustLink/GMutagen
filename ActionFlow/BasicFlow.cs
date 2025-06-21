namespace ActionFlow;

public class BasicFlow<T>(params T[] handlers)
{
    public readonly List<T> Handlers = new();
    public readonly List<T> BeforeHandlers = handlers.ToList();

    public const string HANDLER_FIELD_NAME = nameof(Handlers);
}