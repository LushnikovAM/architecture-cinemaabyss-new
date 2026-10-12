using CinemaAbyss.Events.Configuration;
using CinemaAbyss.Events.Endpoints;
using CinemaAbyss.Events.Messaging;

namespace CinemaAbyss.Events.DependencyInjection;

internal static class EventsServiceCollectionExtensions
{
    public static IServiceCollection AddCinemaAbyssEvents(this IServiceCollection services, EventsSettings settings)
    {
        services.AddSingleton(settings);
        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
        services.AddHostedService<KafkaEventConsumer>();
        services.AddSingleton<EventRequestHandler>();
        return services;
    }
}
