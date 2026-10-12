namespace CinemaAbyss.Proxy.Routing;

internal sealed class UpstreamRouter : IUpstreamRouter
{
    private readonly IReadOnlyList<IUpstreamRoute> _routes;

    public UpstreamRouter(IEnumerable<IUpstreamRoute> routes)
    {
        _routes = routes.OrderBy(route => route.Order).ToArray();
    }

    public Upstream Resolve(string? path)
    {
        var normalized = string.IsNullOrEmpty(path) ? "/" : path;
        foreach (var route in _routes)
        {
            var upstream = route.Match(normalized);
            if (upstream is not null)
            {
                return upstream.Value;
            }
        }

        throw new InvalidOperationException($"No upstream route matched '{normalized}'.");
    }
}
