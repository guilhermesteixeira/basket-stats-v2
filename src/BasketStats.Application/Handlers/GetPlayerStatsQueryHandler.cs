namespace BasketStats.Application.Handlers;

using MediatR;
using BasketStats.Application.DTOs;
using BasketStats.Application.Queries;
using BasketStats.Domain.Abstractions;
using BasketStats.Domain.Entities;

public class GetPlayerStatsQueryHandler(IMatchRepository matchRepository) : IRequestHandler<GetPlayerStatsQuery, PlayerStatsDto?>
{
    public async Task<PlayerStatsDto?> Handle(GetPlayerStatsQuery request, CancellationToken cancellationToken)
    {
        var match = await matchRepository.GetByIdAsync(request.MatchId, cancellationToken);
        if (match is null)
            return null;

        var playerEvents = match.Events.Where(e => e.PlayerId == request.PlayerId).ToList();

        var shotsMade = playerEvents.OfType<ScoreEvent>().Sum(e => e.Points);

        return new PlayerStatsDto
        {
            PlayerId = request.PlayerId,
            MatchId = request.MatchId,
            TotalPoints = shotsMade,
            Fouls = playerEvents.OfType<FoulEvent>().Count(),
            ShotsMade = playerEvents.OfType<ScoreEvent>().Count(),
            ShotsMissed = playerEvents.OfType<MissedShotEvent>().Count(),
            Turnovers = playerEvents.OfType<TurnoverEvent>().Count()
        };
    }
}
