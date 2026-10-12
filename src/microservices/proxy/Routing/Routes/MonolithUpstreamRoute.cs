using CinemaAbyss.Proxy.Configuration;
using CinemaAbyss.Proxy.Routing;

namespace CinemaAbyss.Proxy.Routing.Routes;

internal sealed class MonolithUpstreamRoute : IUpstreamRoute
{
    private readonly ProxySettings _settings;

    public MonolithUpstreamRoute(ProxySettings settings)
    {
        _settings = settings;
    }

    public int Order => RouteOrder.Monolith;

    public Upstream? Match(string path)
    {
        return new Upstream(_settings.MonolithUrl, UpstreamNames.Monolith);
    }
}
