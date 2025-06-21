using Logger.Logger.Interfaces;

namespace EventBus;

public class LoggingMiddleware : IMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(EventContext context, Func<Task> next)
    {
        _logger.LogInfo($"Before event: {context.Event.GetType().Name}");
        await next();
        _logger.LogInfo($"After event: { context.Event.GetType().Name}");
    }
}