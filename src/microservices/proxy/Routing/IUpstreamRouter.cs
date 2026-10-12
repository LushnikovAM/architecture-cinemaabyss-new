namespace CinemaAbyss.Proxy.Routing;

internal interface IUpstreamRouter
{
    Upstream Resolve(string? path);
}
