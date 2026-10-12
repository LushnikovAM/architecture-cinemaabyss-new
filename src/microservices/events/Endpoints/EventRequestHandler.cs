using System.Text.Json;
using CinemaAbyss.Events.Contracts;
using CinemaAbyss.Events.Messaging;

namespace CinemaAbyss.Events.Endpoints;

internal sealed class EventRequestHandler
{
    private readonly IEventPublisher _publisher;
    private readonly ILogger<EventRequestHandler> _logger;

    public EventRequestHandler(IEventPublisher publisher, ILogger<EventRequestHandler> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public Task<IResult> MovieAsync(HttpContext context, CancellationToken cancellationToken)
    {
        return AcceptAsync<MovieEventRequest>(
            context,
            EventTopics.Movie,
            EventTypes.Movie,
            static request => request.Validate(),
            static request => $"movie-{request.MovieId}-{request.Action}",
            static _ => null,
            cancellationToken);
    }

    public Task<IResult> UserAsync(HttpContext context, CancellationToken cancellationToken)
    {
        return AcceptAsync<UserEventRequest>(
            context,
            EventTopics.User,
            EventTypes.User,
            static request => request.Validate(),
            static request => $"user-{request.UserId}-{request.Action}",
            static request => request.ReadTimestamp(),
            cancellationToken);
    }

    public Task<IResult> PaymentAsync(HttpContext context, CancellationToken cancellationToken)
    {
        return AcceptAsync<PaymentEventRequest>(
            context,
            EventTopics.Payment,
            EventTypes.Payment,
            static request => request.Validate(),
            static request => $"payment-{request.PaymentId}-{request.Status}",
            static request => request.ReadTimestamp(),
            cancellationToken);
    }

    private async Task<IResult> AcceptAsync<TRequest>(
        HttpContext context,
        string topic,
        string eventType,
        Func<TRequest, string?> validate,
        Func<TRequest, string> eventId,
        Func<TRequest, DateTimeOffset?> timestamp,
        CancellationToken cancellationToken)
        where TRequest : class
    {
        TRequest? request;
        try
        {
            request = await context.Request.ReadFromJsonAsync<TRequest>(EventsJson.Options, cancellationToken);
        }
        catch (JsonException exception)
        {
            return BadRequest(exception.Message);
        }

        if (request is null)
        {
            return BadRequest("Request body is required.");
        }

        var error = validate(request);
        if (error is not null)
        {
            return BadRequest(error);
        }

        var envelope = EventEnvelope.Create(eventType, eventId(request), request, timestamp(request));

        try
        {
            var delivery = await _publisher.PublishAsync(topic, envelope, cancellationToken);
            return Results.Json(
                new EventAcceptedResponse("success", delivery.Partition, delivery.Offset, envelope),
                EventsJson.Options,
                statusCode: StatusCodes.Status201Created);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Failed to publish {EventType} event to {Topic}", eventType, topic);
            return Results.Json(
                new ErrorResponse("Internal Server Error"),
                EventsJson.Options,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static IResult BadRequest(string error)
    {
        return Results.Json(
            new ErrorResponse(error),
            EventsJson.Options,
            statusCode: StatusCodes.Status400BadRequest);
    }
}
