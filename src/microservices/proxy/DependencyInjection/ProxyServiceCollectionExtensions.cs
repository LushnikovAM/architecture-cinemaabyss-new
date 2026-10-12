using CinemaAbyss.Proxy.Configuration;
using CinemaAbyss.Proxy.Endpoints;
using CinemaAbyss.Proxy.Proxying;
using CinemaAbyss.Proxy.Routing;
using CinemaAbyss.Proxy.Routing.Migration;
using CinemaAbyss.Proxy.Routing.Routes;

namespace CinemaAbyss.Proxy.DependencyInjection;

internal static class ProxyServiceCollectionExtensions
{
    public static IServiceCollection AddCinemaAbyssProxy(this IServiceCollection services, ProxySettings settings)
    {
        services.AddSingleton(settings);
        services.AddSingleton<IMigrationRoll, RandomMigrationRoll>();
        services.AddSingleton<IMoviesMigrationPolicy, GradualMoviesMigrationPolicy>();
        services.AddSingleton<IUpstreamRoute, EventsUpstreamRoute>();
        services.AddSingleton<IUpstreamRoute, MoviesUpstreamRoute>();
        services.AddSingleton<IUpstreamRoute, MonolithUpstreamRoute>();
        services.AddSingleton<IUpstreamRouter, UpstreamRouter>();
        services.AddSingleton<IUpstreamForwarder, HttpUpstreamForwarder>();
        services.AddSingleton<ProxyRequestHandler>();

        services.AddHttpClient(HttpUpstreamForwarder.HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                UseProxy = false,
                PooledConnectionLifetime = TimeSpan.FromMinutes(2)
            });

        return services;
    }
}
