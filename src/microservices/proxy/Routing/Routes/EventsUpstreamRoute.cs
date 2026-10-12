using CinemaAbyss.Proxy.Configuration;
using CinemaAbyss.Proxy.Routing;

namespace CinemaAbyss.Proxy.Routing.Routes;

internal sealed class EventsUpstreamRoute : IUpstreamRoute
{
    private readonly ProxySettings _settings;

    public EventsUpstreamRoute(ProxySettings settings)
    {
        _settings = settings;
    }

    public int Order => RouteOrder.Events;

    public Upstream? Match(string path)
    {
        if (!path.StartsWith("/api/events", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return new Upstream(_settings.EventsServiceUrl, UpstreamNames.Events);
    }
}
