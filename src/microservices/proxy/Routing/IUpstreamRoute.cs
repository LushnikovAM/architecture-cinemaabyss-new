namespace CinemaAbyss.Proxy.Routing;

internal interface IUpstreamRoute
{
    int Order { get; }

    Upstream? Match(string path);
}

internal static class RouteOrder
{
    public const int Events = 100;
    public const int Movies = 200;
    public const int Monolith = 1000;
}
