using CinemaAbyss.Proxy.Configuration;
using CinemaAbyss.Proxy.DependencyInjection;
using CinemaAbyss.Proxy.Endpoints;

var settings = ProxySettings.FromEnvironment();
Environment.SetEnvironmentVariable("ASPNETCORE_HTTP_PORTS", null);
Environment.SetEnvironmentVariable("ASPNETCORE_HTTPS_PORTS", null);
Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://+:{settings.Port}");

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
builder.Services.AddCinemaAbyssProxy(settings);

var app = builder.Build();

app.Logger.LogInformation(
    "CinemaAbyss proxy on port {Port}. Monolith={Monolith}, Movies={Movies}, Events={Events}, GradualMigration={Gradual}, MoviesMigrationPercent={Percent}",
    settings.Port,
    settings.MonolithUrl,
    settings.MoviesServiceUrl,
    settings.EventsServiceUrl,
    settings.GradualMigration,
    settings.MoviesMigrationPercent);

app.MapGet("/health", HealthEndpoint.Handle);
app.MapFallback((HttpContext context, ProxyRequestHandler handler) => handler.HandleAsync(context));

app.Run();
