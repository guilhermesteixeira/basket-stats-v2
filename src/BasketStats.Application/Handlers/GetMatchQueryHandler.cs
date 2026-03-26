namespace BasketStats.Application.Handlers;

using MediatR;
using BasketStats.Application.DTOs;
using BasketStats.Application.Mapping;
using BasketStats.Application.Queries;
using BasketStats.Domain.Abstractions;

public class GetMatchQueryHandler(IMatchRepository matchRepository, ITeamRepository teamRepository)
    : IRequestHandler<GetMatchQuery, MatchDto?>
{
    public async Task<MatchDto?> Handle(GetMatchQuery request, CancellationToken cancellationToken)
    {
        var match = await matchRepository.GetByIdAsync(request.MatchId, cancellationToken);
        if (match is null) return null;

        var homeTeam = await teamRepository.GetByIdAsync(match.HomeTeamId, cancellationToken);
        var awayTeam = await teamRepository.GetByIdAsync(match.AwayTeamId, cancellationToken);

        return MatchMapper.ToDto(
            match,
            homeTeam?.Name ?? match.HomeTeamId,
            awayTeam?.Name ?? match.AwayTeamId
        );
    }
}
