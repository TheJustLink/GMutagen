using Logger.Logger.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus;

public class EventBusBuilder
{
    private readonly List<IMiddleware> _middlewares = new();
    private readonly IServiceProvider _provider;

    public EventBusBuilder(IServiceProvider provider)
    {
        _provider = provider;
    }

    public EventBusBuilder Use<TMiddleware>() where TMiddleware : IMiddleware
    {
        var instance = (IMiddleware)ActivatorUtilities.CreateInstance(_provider, typeof(TMiddleware));
        _middlewares.Add(instance);
        return this;
    }

    public EventBusBuilder Use(IMiddleware middleware)
    {
        _middlewares.Add(middleware);
        return this;
    }

    public EventBusBuilder Use(Func<EventContext, Func<Task>, Task> func)
    {
        _middlewares.Add(new DelegateMiddleware(func));
        return this;
    }

    public EventBus Build()
    {
        var logger = _provider.GetRequiredService<ILogger<EventBus>>();
        return new EventBus(_middlewares, logger).Build();
    }
}