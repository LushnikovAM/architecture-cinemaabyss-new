namespace CinemaAbyss.Proxy.Endpoints;

internal static class HealthEndpoint
{
    public static IResult Handle()
    {
        return Results.Text("Strangler Fig Proxy is healthy", "text/plain");
    }
}
