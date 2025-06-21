using System.Collections.Concurrent;
using Logger.Logger;
using Logger.Logger.Interfaces;

namespace EventBus;

public class EventBus(
    ConcurrentDictionary<Topic, ConcurrentBag<ISubscription>> subscriptions,
    List<IMiddleware> middlewares,
    ILogger<EventBus> logger)
{
    private readonly ConcurrentDictionary<Topic, ConcurrentBag<ISubscription>> _subscriptions = subscriptions;
    private readonly List<IMiddleware> _middlewares = middlewares;
    private readonly ILogger<EventBus> _logger = logger;

    public const string SUBSCRIPTIONS_FIELD_NAME = nameof(_subscriptions);

    public EventBus(List<IMiddleware> middlewares, ILogger<EventBus> logger)
        : this(new(), middlewares, logger)
    {
    }

    public EventBus(ILogger<EventBus> logger)
        : this(new(), new(), logger)
    {
    }

    public IDisposable Subscribe<TEvent>(
        Topic topic,
        IEventHandler<TEvent> handler,
        IEventFilter<TEvent>? filter = null
    ) where TEvent : Event
    {
        var subscription = new Subscription<TEvent>(
            topic,
            handler,
            filter,
            typeof(TEvent)
        );

        lock (_subscriptions)
        {
            if (!_subscriptions.ContainsKey(topic))
                _subscriptions[topic] = new ConcurrentBag<ISubscription>();

            _subscriptions[topic].Add(subscription);
        }

        _logger.LogDebug($"Subscribed to topic {topic} for event {typeof(TEvent).Name}");
        return new Disposable(() => Unsubscribe(subscription));
    }

    public IDisposable Subscribe<TEvent>(
        Topic topic,
        Action<TEvent> handler,
        Func<TEvent, bool>? filter = null
    ) where TEvent : Event
    {
        var actionHandler = new ActionHandler<TEvent>(handler);
        if (filter != null)
        {
            var funcFilter = new FuncFilter<TEvent>(filter);
            return Subscribe(topic, actionHandler, funcFilter);
        }

        return Subscribe(topic, actionHandler, null);
    }

    public IDisposable Replicate(Topic source, params Topic[] targets)
    {
        var disposables = targets
            .Select(target =>
            {
                var handler = new ReplicateHandler(this, target, new ConsoleLogger<ReplicateHandler>());
                return Subscribe(source, handler);
            })
            .ToList();

        return new CompositeDisposable(disposables);
    }

    public IDisposable MergeTopics(Topic target, params Topic[] sources)
    {
        var disposables = sources
            .Select(source =>
            {
                var handler = new MergeHandler(this, target, new ConsoleLogger<MergeHandler>());
                return Subscribe(source, handler);
            })
            .ToList();

        return new CompositeDisposable(disposables);
    }


    public async Task Publish<TEvent>(TEvent @event, Topic topic) where TEvent : Event
    {
        var context = new EventContext(@event, topic);
        var pipeline = () => DispatchEvent(context);

        foreach (var middleware in _middlewares)
        {
            var next = pipeline;
            pipeline = () => middleware.InvokeAsync(context, next);
        }

        _logger.LogDebug($"Publishing event of type {typeof(TEvent).Name} to topic {topic}");
        await pipeline();
    }

    private async Task DispatchEvent(EventContext context)
    {
        if (context.IsHandled) return;

        List<ISubscription> matched;
        lock (_subscriptions)
        {
            matched = _subscriptions
                .Where(kvp => kvp.Key.Matches(context.Topic))
                .SelectMany(kvp => kvp.Value)
                .Where(s => s.EventType.IsInstanceOfType(context.Event))
                .Where(s => s.Check(context.Event))
                .ToList();
        }

        foreach (var subscription in matched)
        {
            try
            {
                await subscription.Handle(context.Event);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error dispatching event to subscription: {ex}");
            }
        }
    }

    public EventBus Build()
    {
        _middlewares.Reverse();
        return this;
    }

    private void Unsubscribe(ISubscription subscription)
    {
        lock (_subscriptions)
        {
            foreach (var kvp in _subscriptions.Where(x => x.Value.Contains(subscription)))
                _subscriptions[kvp.Key] = new ConcurrentBag<ISubscription>(kvp.Value.Where(s => s != subscription));
        }

        _logger.LogDebug($"Unsubscribed from topic {subscription.Topic}");
    }
    

    private class Disposable(Action action) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            action();
        }
    }
}

public class Subscription<TEvent> : ISubscription where TEvent : Event
{
    public Topic Topic { get; }
    public Type EventType { get; }
    public object HandlerInstance => Handler;
    public object FilterInstance => Filter;
    private IEventHandler<TEvent> Handler { get; }
    private IEventFilter<TEvent>? Filter { get; }

    public Subscription(Topic topic, IEventHandler<TEvent> handler, IEventFilter<TEvent>? filter, Type eventType)
    {
        Topic = topic;
        Handler = handler;
        Filter = filter;
        EventType = eventType;
    }

    public async Task Handle(Event e)
    {
        await Handler.HandleAsync((TEvent)e);
    }

    public bool Check(Event e)
    {
        if (Filter is null)
            return true;

        return Filter.Filter((TEvent)e);
    }
}

public interface ISubscription
{
    Topic Topic { get; }
    Type EventType { get; }
    object HandlerInstance { get; }
    object FilterInstance { get; }

    Task Handle(Event e);
    bool Check(Event e);
}