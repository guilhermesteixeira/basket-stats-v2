namespace BasketStats.API.Requests;

using BasketStats.Domain.Enums;
using BasketStats.Domain.ValueObjects;

public record AddEventRequest
{
    public required string TeamId { get; init; }
    public string? PlayerId { get; init; }
    public required EventType Type { get; init; }
    public required PeriodNumber PeriodNumber { get; init; }
    public required int PeriodTimestamp { get; init; }
    public decimal? CoordinatesX { get; init; }
    public decimal? CoordinatesY { get; init; }
    public int? Points { get; init; }
    public FoulType? FoulType { get; init; }
    public string? PlayerFouledId { get; init; }
    public bool? Flagrant { get; init; }
    public string? PlayerOutId { get; init; }
}
