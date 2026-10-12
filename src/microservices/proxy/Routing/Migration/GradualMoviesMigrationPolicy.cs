using CinemaAbyss.Proxy.Configuration;
using CinemaAbyss.Proxy.Routing;

namespace CinemaAbyss.Proxy.Routing.Migration;

internal sealed class GradualMoviesMigrationPolicy : IMoviesMigrationPolicy
{
    private readonly ProxySettings _settings;
    private readonly IMigrationRoll _roll;

    public GradualMoviesMigrationPolicy(ProxySettings settings, IMigrationRoll roll)
    {
        _settings = settings;
        _roll = roll;
    }

    public bool UseMoviesService()
    {
        if (!_settings.GradualMigration)
        {
            return true;
        }

        return _roll.NextPercent() < _settings.MoviesMigrationPercent;
    }
}
