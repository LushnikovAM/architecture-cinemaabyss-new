using CinemaAbyss.Events.Contracts;

namespace CinemaAbyss.Events.Messaging;

internal interface IEventPublisher
{
    Task<EventDelivery> PublishAsync(string topic, EventEnvelope envelope, CancellationToken cancellationToken);
}

internal readonly record struct EventDelivery(int Partition, long Offset);
