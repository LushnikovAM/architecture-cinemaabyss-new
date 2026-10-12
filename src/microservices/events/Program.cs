using CinemaAbyss.Events.Configuration;
using CinemaAbyss.Events.DependencyInjection;
using CinemaAbyss.Events.Endpoints;

var settings = EventsSettings.FromEnvironment();
Environment.SetEnvironmentVariable("ASPNETCORE_HTTP_PORTS", null);
Environment.SetEnvironmentVariable("ASPNETCORE_HTTPS_PORTS", null);
Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://+:{settings.Port}");

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
builder.Logging.AddFilter("Confluent", LogLevel.Warning);
builder.Services.AddCinemaAbyssEvents(settings);

var app = builder.Build();

app.Logger.LogInformation(
    "CinemaAbyss events on port {Port}. Kafka={Brokers}",
    settings.Port,
    settings.KafkaBrokers);

app.MapGet("/api/events/health", HealthEndpoint.Handle);
app.MapPost("/api/events/movie", (HttpContext context, EventRequestHandler handler, CancellationToken cancellationToken)
    => handler.MovieAsync(context, cancellationToken));
app.MapPost("/api/events/user", (HttpContext context, EventRequestHandler handler, CancellationToken cancellationToken)
    => handler.UserAsync(context, cancellationToken));
app.MapPost("/api/events/payment", (HttpContext context, EventRequestHandler handler, CancellationToken cancellationToken)
    => handler.PaymentAsync(context, cancellationToken));

app.Run();
