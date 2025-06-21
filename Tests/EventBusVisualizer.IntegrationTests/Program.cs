using System.Collections.Concurrent;
using System.Reflection;
using EventBus;
using Logger.Logger;

namespace EventBusVisualizer.IntegrationTests;

public class Program
{
    public static void Main()
    {
        var logger = new ConsoleLogger<EventBus.EventBus>();
        var loggerMiddleware = new ConsoleLogger<LoggingMiddleware>();

        var middlweares = new List<IMiddleware>()
        {
            new LoggingMiddleware(loggerMiddleware),
        };
        
        var eventBus = new EventBus.EventBus(middlweares, logger);

        eventBus.Subscribe<OrderPlacedEvent>(Topic.Parse("orders/new"),
            async e => { Console.WriteLine("Order handler"); });

        eventBus.Subscribe<PaymentCompletedEvent>(Topic.Parse("payments/complete"),
            async e => { Console.WriteLine("Payment handler"); });

        eventBus.AddReplicateGroup("orders/new")
            .To("analytics/orders")
            .To("notifications/orders")
            .Build();

        eventBus.AddMergeGroup("monitoring/all")
            .From("orders/new")
            .From("payments/complete")
            .Build();


        var graph = EventBusGraphBuilder.BuildFrom(eventBus);

        var dotOutput = TopicGraphExporter.ToDot(graph);
        File.WriteAllText("eventbus.dot", dotOutput);

        Console.WriteLine("DOT файл сохранён как 'eventbus.dot'");

        eventBus.Publish(new PaymentCompletedEvent(), "payments/complete");
        eventBus.Publish(new UserNotificationEvent(), "monitoring/all");
    }
}

public class OrderPlacedEvent : Event
{
}

public class PaymentCompletedEvent : Event
{
}

public class UserNotificationEvent : Event
{
}

public static class EventBusGraphBuilder
{
    public static EventBusGraph BuildFrom(EventBus.EventBus bus)
    {
        var graph = new EventBusGraph();

        var subscriptionsField = typeof(EventBus.EventBus)
            .GetField(EventBus.EventBus.SUBSCRIPTIONS_FIELD_NAME, BindingFlags.NonPublic | BindingFlags.Instance);

        if (subscriptionsField?.GetValue(bus) is not ConcurrentDictionary<Topic, ConcurrentBag<ISubscription>>
            subscriptions)
            return graph;

        foreach (var (topic, handlers) in subscriptions)
        {
            graph.AddTopic(topic);

            foreach (var subscription in handlers)
            {
                graph.AddTopic(subscription.Topic);

                var handlerInstance = subscription.HandlerInstance;
                if (handlerInstance == null)
                    continue;

                var handlerType = handlerInstance.GetType();


                if (handlerInstance is ReplicateHandler replicateHandler)
                {
                    var targetField = handlerType
                        .GetField(ReplicateHandler.TARGET_FIELD_NAME, BindingFlags.NonPublic | BindingFlags.Instance);
                    if (targetField?.GetValue(replicateHandler) is Topic targetTopic)
                    {
                        graph.AddTopic(targetTopic);
                        graph.AddReplication(subscription.Topic.Pattern, targetTopic.Pattern);
                    }
                }

                else if (handlerInstance is MergeHandler mergeHandler)
                {
                    var targetField = handlerType
                        .GetField(MergeHandler.TARGET_FIELD_NAME, BindingFlags.NonPublic | BindingFlags.Instance);
                    if (targetField?.GetValue(mergeHandler) is Topic targetTopic)
                    {
                        graph.AddTopic(targetTopic);
                        graph.AddMerge(targetTopic.Pattern, subscription.Topic.Pattern);
                    }
                }
            }
        }

        return graph;
    }
}