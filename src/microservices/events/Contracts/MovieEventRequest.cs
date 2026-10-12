using System.Text.Json.Serialization;

namespace CinemaAbyss.Events.Contracts;

internal sealed class MovieEventRequest
{
    [JsonRequired]
    public int MovieId { get; init; }

    [JsonRequired]
    public string Title { get; init; } = "";

    [JsonRequired]
    public string Action { get; init; } = "";

    public int? UserId { get; init; }

    public double? Rating { get; init; }

    public IReadOnlyList<string>? Genres { get; init; }

    public string? Description { get; init; }

    public string? Validate()
    {
        if (MovieId <= 0)
        {
            return "movie_id must be a positive integer.";
        }

        if (string.IsNullOrWhiteSpace(Title))
        {
            return "title is required.";
        }

        if (string.IsNullOrWhiteSpace(Action))
        {
            return "action is required.";
        }

        return null;
    }
}
