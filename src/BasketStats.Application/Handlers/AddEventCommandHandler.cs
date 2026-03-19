namespace BasketStats.Application.Handlers;

using MediatR;
using BasketStats.Application.Commands;
using BasketStats.Application.Exceptions;
using BasketStats.Domain.Abstractions;
using BasketStats.Domain.Entities;
using BasketStats.Domain.Enums;
using BasketStats.Domain.ValueObjects;

public class AddEventCommandHandler(
    IMatchRepository matchRepository,
    ITeamRepository teamRepository,
    IUserRepository userRepository) : IRequestHandler<AddEventCommand, string>
{
    public async Task<string> Handle(AddEventCommand request, CancellationToken cancellationToken)
    {
        var match = await matchRepository.GetByIdAsync(request.MatchId, cancellationToken);
        if (match is null)
            throw new NotFoundException($"Match '{request.MatchId}' not found");

        if (match.Status != Domain.Enums.MatchStatus.Active)
            throw new InvalidOperationException("Cannot add events to a non-active match");

        var user = await userRepository.GetByIdAsync(request.RequestedByUserId, cancellationToken);
        if (user is null)
            throw new NotFoundException($"User '{request.RequestedByUserId}' not found");

        var homeTeam = await teamRepository.GetByIdAsync(match.HomeTeamId, cancellationToken);
        var awayTeam = await teamRepository.GetByIdAsync(match.AwayTeamId, cancellationToken);

        var isHomeOwner = homeTeam?.IsOwnedBy(request.RequestedByUserId) ?? false;
        var isAwayOwner = awayTeam?.IsOwnedBy(request.RequestedByUserId) ?? false;

        bool isAuthorized = false;

        if (user.IsAdmin)
        {
            isAuthorized = true;
        }
        else if (isHomeOwner && request.TeamId == match.HomeTeamId)
        {
            isAuthorized = true;
        }
        else if (isAwayOwner && request.TeamId == match.AwayTeamId)
        {
            isAuthorized = true;
        }
        else if (isHomeOwner && request.TeamId == match.AwayTeamId)
        {
            isAuthorized = IsOpponentTrackingAllowed(request.Type);
        }
        else if (isAwayOwner && request.TeamId == match.HomeTeamId)
        {
            isAuthorized = IsOpponentTrackingAllowed(request.Type);
        }

        if (!isAuthorized)
            throw new ForbiddenException("User is not authorized to add events to this match");

        var @event = CreateEvent(request);
        match.AddEvent(@event);
        await matchRepository.SaveAsync(match, cancellationToken);

        return @event.Id.Value;
    }

    private static bool IsOpponentTrackingAllowed(EventType type) =>
        type is EventType.Score or EventType.MissedShot or EventType.FreeThrow;

    private static Event CreateEvent(AddEventCommand cmd) =>
        cmd.Type switch
        {
            EventType.Score => new ScoreEvent(
                cmd.TeamId,
                cmd.PlayerId,
                cmd.Points!.Value,
                new Coordinates(cmd.CoordinatesX!.Value, cmd.CoordinatesY!.Value),
                cmd.PeriodNumber,
                cmd.PeriodTimestamp),

            EventType.MissedShot => new MissedShotEvent(
                cmd.TeamId,
                cmd.PlayerId,
                new Coordinates(cmd.CoordinatesX!.Value, cmd.CoordinatesY!.Value),
                cmd.PeriodNumber,
                cmd.PeriodTimestamp),

            EventType.FreeThrow => new FreeThrowEvent(
                cmd.TeamId,
                cmd.PlayerId,
                cmd.Made!.Value,
                cmd.FoulType!.Value,
                cmd.PeriodNumber,
                cmd.PeriodTimestamp),

            EventType.Foul => new FoulEvent(
                cmd.TeamId,
                cmd.PlayerId,
                cmd.FoulType!.Value,
                cmd.PlayerFouledId!,
                cmd.Flagrant!.Value,
                cmd.PeriodNumber,
                cmd.PeriodTimestamp),

            EventType.Substitution => new SubstitutionEvent(
                cmd.TeamId,
                cmd.PlayerId,
                cmd.PlayerOutId!,
                cmd.PeriodNumber,
                cmd.PeriodTimestamp),

            _ => throw new InvalidOperationException($"Unknown event type: {cmd.Type}")
        };
}
