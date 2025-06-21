using Logger.Logger.Interfaces;

namespace EventBus;

public class ReplicateHandler : IEventHandler<Event>
{
    public const string TARGET_FIELD_NAME = nameof(_target);
    
    private readonly EventBus _bus;
    private readonly Topic _target;
    private readonly ILogger<ReplicateHandler> _logger;

    public ReplicateHandler(EventBus bus, Topic target, ILogger<ReplicateHandler> logger)
    {
        _bus = bus;
        _target = target;
        _logger = logger;
    }

    public async Task HandleAsync(Event @event)
    {
        _logger.LogDebug($"Replicating event {@event.GetType().Name} to topic {_target}");
        await _bus.Publish(@event, _target);
    }
}