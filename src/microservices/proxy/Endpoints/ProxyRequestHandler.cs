using CinemaAbyss.Proxy.Proxying;
using CinemaAbyss.Proxy.Routing;

namespace CinemaAbyss.Proxy.Endpoints;

internal sealed class ProxyRequestHandler
{
    private readonly IUpstreamRouter _router;
    private readonly IUpstreamForwarder _forwarder;
    private readonly ILogger<ProxyRequestHandler> _logger;

    public ProxyRequestHandler(
        IUpstreamRouter router,
        IUpstreamForwarder forwarder,
        ILogger<ProxyRequestHandler> logger)
    {
        _router = router;
        _forwarder = forwarder;
        _logger = logger;
    }

    public Task HandleAsync(HttpContext context)
    {
        var upstream = _router.Resolve(context.Request.Path.Value);
        _logger.LogInformation(
            "{Method} {Path} -> {Upstream}",
            context.Request.Method,
            context.Request.Path + context.Request.QueryString,
            upstream.Name);

        return _forwarder.ForwardAsync(context, upstream);
    }
}
