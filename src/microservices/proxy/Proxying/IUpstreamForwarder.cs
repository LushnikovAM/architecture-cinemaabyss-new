using CinemaAbyss.Proxy.Routing;

namespace CinemaAbyss.Proxy.Proxying;

internal interface IUpstreamForwarder
{
    Task ForwardAsync(HttpContext context, Upstream upstream);
}
