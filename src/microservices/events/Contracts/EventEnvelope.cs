using System.Text.Json;

namespace CinemaAbyss.Events.Contracts;

internal sealed record EventEnvelope(string Id, string Type, DateTimeOffset Timestamp, JsonElement Payload)
{
    public static EventEnvelope Create(string type, string id, object payload, DateTimeOffset? timestamp)
    {
        var uniqueId = $"{id}-{Guid.NewGuid().ToString("N")[..8]}";
        return new EventEnvelope(
            uniqueId,
            type,
            timestamp ?? DateTimeOffset.UtcNow,
            JsonSerializer.SerializeToElement(payload, EventsJson.Options));
    }
}
