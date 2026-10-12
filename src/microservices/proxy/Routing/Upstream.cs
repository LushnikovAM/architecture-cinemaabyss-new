namespace CinemaAbyss.Proxy.Routing;

internal readonly record struct Upstream(string BaseUrl, string Name);

internal static class UpstreamNames
{
    public const string Monolith = "monolith";
    public const string Movies = "movies-service";
    public const string Events = "events-service";
}
