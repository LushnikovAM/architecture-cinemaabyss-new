using CinemaAbyss.Events.Contracts;

namespace CinemaAbyss.Events.Endpoints;

internal static class HealthEndpoint
{
    public static IResult Handle()
    {
        return Results.Json(new HealthResponse(true));
    }
}
