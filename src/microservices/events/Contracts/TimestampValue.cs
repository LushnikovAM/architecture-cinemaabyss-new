using System.Globalization;

namespace CinemaAbyss.Events.Contracts;

internal static class TimestampValue
{
    public static bool TryParse(string? value, out DateTimeOffset timestamp)
    {
        return DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out timestamp);
    }
}
