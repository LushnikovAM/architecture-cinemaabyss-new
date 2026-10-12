using System.Text.Json.Serialization;

namespace CinemaAbyss.Events.Contracts;

internal sealed class PaymentEventRequest
{
    [JsonRequired]
    public int PaymentId { get; init; }

    [JsonRequired]
    public int UserId { get; init; }

    [JsonRequired]
    public double Amount { get; init; }

    [JsonRequired]
    public string Status { get; init; } = "";

    [JsonRequired]
    public string Timestamp { get; init; } = "";

    public string? MethodType { get; init; }

    public string? Validate()
    {
        if (PaymentId <= 0)
        {
            return "payment_id must be a positive integer.";
        }

        if (UserId <= 0)
        {
            return "user_id must be a positive integer.";
        }

        if (double.IsNaN(Amount) || double.IsInfinity(Amount))
        {
            return "amount must be a finite number.";
        }

        if (string.IsNullOrWhiteSpace(Status))
        {
            return "status is required.";
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
