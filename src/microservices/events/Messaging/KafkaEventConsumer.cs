using System.Text.Json;
using CinemaAbyss.Events.Configuration;
using Confluent.Kafka;

namespace CinemaAbyss.Events.Messaging;

internal sealed class KafkaEventConsumer : BackgroundService
{
    private readonly EventsSettings _settings;
    private readonly ILogger<KafkaEventConsumer> _logger;

    public KafkaEventConsumer(EventsSettings settings, ILogger<KafkaEventConsumer> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Factory.StartNew(
            () => RunAsync(stoppingToken),
            stoppingToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default).Unwrap();
    }

    private async Task RunAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                Consume(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Kafka consumer stopped. Retry in 5 seconds");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private void Consume(CancellationToken stoppingToken)
    {
        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = _settings.KafkaBrokers,
            GroupId = "cinemaabyss-events",
            ClientId = "cinemaabyss-events-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            AllowAutoCreateTopics = true,
            SocketTimeoutMs = 10000
        }).Build();

        consumer.Subscribe(EventTopics.All);
        _logger.LogInformation("Kafka consumer subscribed to {Topics}", string.Join(", ", EventTopics.All));

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);
                if (result?.Message?.Value is null)
                {
                    continue;
                }

                _logger.LogInformation(
                    "Processed {EventType} event from topic {Topic} partition {Partition} offset {Offset}: {Payload}",
                    ReadEventType(result.Message.Value),
                    result.Topic,
                    result.Partition.Value,
                    result.Offset.Value,
                    result.Message.Value);

                consumer.Commit(result);
            }
        }
        finally
        {
            try
            {
                consumer.Close();
            }
            catch (KafkaException exception)
            {
                _logger.LogDebug(exception, "Kafka consumer close failed");
            }
        }
    }

    private static string ReadEventType(string payload)
    {
        try
        {
            using var document = JsonDocument.Parse(payload);
            if (document.RootElement.TryGetProperty("type", out var type))
            {
                return type.GetString() ?? "unknown";
            }
        }
        catch (JsonException)
        {
            return "unknown";
        }

        return "unknown";
    }
}
