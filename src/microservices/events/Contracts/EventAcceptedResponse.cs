namespace CinemaAbyss.Events.Contracts;

internal sealed record EventAcceptedResponse(string Status, int Partition, long Offset, EventEnvelope Event);
