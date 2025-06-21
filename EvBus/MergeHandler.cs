using Logger.Logger.Interfaces;

namespace EventBus;

public class MergeHandler : IEventHandler<Event>
{
    public const string TARGET_FIELD_NAME = nameof(_target);
    
    private readonly EventBus _bus;
    private readonly Topic _target;
    private readonly ILogger<MergeHandler> _logger;

    public MergeHandler(EventBus bus, Topic target, ILogger<MergeHandler> logger)
    {
        _bus = bus;
        _target = target;
        _logger = logger;
    }

    public async Task HandleAsync(Event @event)
    {
        _logger.LogDebug($"Merging event {@event.GetType().Name} into topic {_target}");
        await _bus.Publish(@event, _target);
    }
}