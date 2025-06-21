using Microsoft.Extensions.DependencyInjection;

namespace EventBus;

public static class EventBusServiceCollectionExtensions
{
    public static IServiceCollection AddEventBus(this IServiceCollection services, Action<EventBusBuilder> configure)
    {
        services.AddSingleton<EventBus>(sp =>
        {
            var builder = new EventBusBuilder(sp);
            configure(builder);
            return builder.Build();
        });

        return services;
    }
}