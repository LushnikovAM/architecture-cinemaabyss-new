namespace CinemaAbyss.Proxy.Configuration;

internal sealed record ProxySettings(
    int Port,
    string MonolithUrl,
    string MoviesServiceUrl,
    string EventsServiceUrl,
    bool GradualMigration,
    int MoviesMigrationPercent)
{
    public static ProxySettings FromEnvironment()
    {
        return new ProxySettings(
            ReadPort("PORT"),
            ReadUrl("MONOLITH_URL"),
            ReadUrl("MOVIES_SERVICE_URL"),
            ReadUrl("EVENTS_SERVICE_URL"),
            ReadFlag("GRADUAL_MIGRATION"),
            ReadPercent("MOVIES_MIGRATION_PERCENT"));
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

    private static string ReadUrl(string name)
    {
        return ReadRequired(name).TrimEnd('/');
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

    private static bool ReadFlag(string name)
    {
        var value = ReadRequired(name);
        if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        throw new InvalidOperationException($"Environment variable {name} must be true or false.");
    }

    private static int ReadPercent(string name)
    {
        var value = ReadRequired(name);
        if (!int.TryParse(value, out var percent) || percent is < 0 or > 100)
        {
            throw new InvalidOperationException($"Environment variable {name} must be an integer from 0 to 100.");
        }

        return percent;
    }
}
