using Logger.Logger;
using Xunit;

namespace EventBus;

public class EventBusTests
{
    [Fact]
    public async Task Should_Invoke_Handler_When_Topic_Matches()
    {
        var logger = new ConsoleLogger<EventBus>();
        var eventBus = new EventBus(new List<IMiddleware>(), logger);
        var called = false;

        var topic = new Topic("test/event");

        eventBus.Subscribe<TestEvent>(topic, async _ => called = true);

      
        await eventBus.Publish(new TestEvent { Topic = "test/event" }, topic);
        
        Assert.True(called);
    }

    [Fact]
    public async Task Should_Not_Invoke_Handler_When_Filter_Fails()
    {
        var logger = new ConsoleLogger<EventBus>();
        var eventBus = new EventBus(new List<IMiddleware>(), logger);
        var called = false;

        var topic = new Topic("filter/test");

        eventBus.Subscribe<TestEvent>(topic, async _ => called = true, e => false);

        await eventBus.Publish(new TestEvent { Topic = "filter/test" }, topic);

        Assert.False(called);
    }

    [Fact]
    public async Task Should_Invoke_Middleware()
    {
        var logger = new ConsoleLogger<EventBus>();
        var called = false;

        var middleware = new DelegateMiddleware((ctx, next) =>
        {
            called = true;
            return next();
        });

        var eventBus = new EventBus(new List<IMiddleware> { middleware }, logger);
        var topic = new Topic("middleware/test");

        eventBus.Subscribe<TestEvent>(topic, async _ => { });

        await eventBus.Publish(new TestEvent { Topic = "middleware/test" }, topic);

        Assert.True(called);
    }

    private class TestEvent : Event { }
}