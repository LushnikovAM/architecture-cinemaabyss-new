namespace CinemaAbyss.Proxy.Routing;

internal sealed class RandomMigrationRoll : IMigrationRoll
{
    public int NextPercent() => Random.Shared.Next(100);
}
