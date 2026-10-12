using CinemaAbyss.Proxy.Configuration;
using CinemaAbyss.Proxy.Routing;
using CinemaAbyss.Proxy.Routing.Migration;

namespace CinemaAbyss.Proxy.Routing.Routes;

internal sealed class MoviesUpstreamRoute : IUpstreamRoute
{
    private readonly ProxySettings _settings;
    private readonly IMoviesMigrationPolicy _policy;

    public MoviesUpstreamRoute(ProxySettings settings, IMoviesMigrationPolicy policy)
    {
        _settings = settings;
        _policy = policy;
    }

    public int Order => RouteOrder.Movies;

    public Upstream? Match(string path)
    {
        if (!path.StartsWith("/api/movies", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var useMoviesService = IsMoviesHealth(path) || _policy.UseMoviesService();
        return useMoviesService
            ? new Upstream(_settings.MoviesServiceUrl, UpstreamNames.Movies)
            : new Upstream(_settings.MonolithUrl, UpstreamNames.Monolith);
    }

    private static bool IsMoviesHealth(string path)
    {
        return path.StartsWith("/api/movies/health", StringComparison.OrdinalIgnoreCase);
    }
}
