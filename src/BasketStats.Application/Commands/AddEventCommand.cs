namespace BasketStats.Application.Commands;

using MediatR;
using BasketStats.Domain.Enums;
using BasketStats.Domain.ValueObjects;

public record AddEventCommand : IRequest<string>
{
    public required string MatchId { get; init; }
    public required string TeamId { get; init; }
    public required string PlayerId { get; init; }
    public required EventType Type { get; init; }
    public required PeriodNumber PeriodNumber { get; init; }
    public required int PeriodTimestamp { get; init; }
    public required string RequestedByUserId { get; init; }
    // Score & MissedShot
    public decimal? CoordinatesX { get; init; }
    public decimal? CoordinatesY { get; init; }
    // Score only
    public int? Points { get; init; }
    // FreeThrow
    public bool? Made { get; init; }
    public FoulType? FoulType { get; init; }
    // Foul
    public string? PlayerFouledId { get; init; }
    public bool? Flagrant { get; init; }
    // Substitution
    public string? PlayerOutId { get; init; }
}
