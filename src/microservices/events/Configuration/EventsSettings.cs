namespace CinemaAbyss.Events.Configuration;

internal sealed record EventsSettings(int Port, string KafkaBrokers)
{
    public static EventsSettings FromEnvironment()
    {
        return new EventsSettings(ReadPort("PORT"), ReadRequired("KAFKA_BROKERS"));
    }

    private static string ReadRequired(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Environment variable {name} is required.");
        }

        return value.Trim();
    }

    private static int ReadPort(string name)
    {
        var value = ReadRequired(name);
        if (!int.TryParse(value, out var port) || port is < 1 or > 65535)
        {
            throw new InvalidOperationException($"Environment variable {name} must be a TCP port from 1 to 65535.");
        }

        return port;
    }
}
