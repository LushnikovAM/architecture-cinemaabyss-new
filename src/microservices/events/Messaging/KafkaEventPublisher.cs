using System.Text.Json;
using CinemaAbyss.Events.Configuration;
using CinemaAbyss.Events.Contracts;
using Confluent.Kafka;

namespace CinemaAbyss.Events.Messaging;

internal sealed class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;

    public KafkaEventPublisher(EventsSettings settings, ILogger<KafkaEventPublisher> logger)
    {
        _logger = logger;
        _producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = settings.KafkaBrokers,
            ClientId = "cinemaabyss-events-producer",
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 5,
            RetryBackoffMs = 200,
            MessageTimeoutMs = 30000,
            SocketTimeoutMs = 10000
        }).Build();
    }

    public async Task<EventDelivery> PublishAsync(
        string topic,
        EventEnvelope envelope,
        CancellationToken cancellationToken)
    {
        var message = new Message<string, string>
        {
            Key = envelope.Id,
            Value = JsonSerializer.Serialize(envelope, EventsJson.Options)
        };

        var result = await _producer.ProduceAsync(topic, message, cancellationToken);
        _logger.LogInformation(
            "Published {EventType} event {EventId} to {Topic} partition {Partition} offset {Offset}",
            envelope.Type,
            envelope.Id,
            topic,
            result.Partition.Value,
            result.Offset.Value);

        return new EventDelivery(result.Partition.Value, result.Offset.Value);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
