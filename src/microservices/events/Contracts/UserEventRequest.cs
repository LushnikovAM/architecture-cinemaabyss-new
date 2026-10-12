using System.Text.Json.Serialization;

namespace CinemaAbyss.Events.Contracts;

internal sealed class UserEventRequest
{
    [JsonRequired]
    public int UserId { get; init; }

    public string? Username { get; init; }

    public string? Email { get; init; }

    [JsonRequired]
    public string Action { get; init; } = "";

    [JsonRequired]
    public string Timestamp { get; init; } = "";

    public string? Validate()
    {
        if (UserId <= 0)
        {
            return "user_id must be a positive integer.";
        }

        if (string.IsNullOrWhiteSpace(Action))
        {
            return "action is required.";
        }

        if (!TimestampValue.TryParse(Timestamp, out _))
        {
            return "timestamp must be an ISO-8601 date-time.";
        }

        return null;
    }

    public DateTimeOffset ReadTimestamp()
    {
        TimestampValue.TryParse(Timestamp, out var timestamp);
        return timestamp;
    }
}
