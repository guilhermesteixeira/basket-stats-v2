namespace BasketStats.Application.Handlers;

using MediatR;
using BasketStats.Application.DTOs;
using BasketStats.Application.Mapping;
using BasketStats.Application.Queries;
using BasketStats.Domain.Abstractions;

public class ListMatchesQueryHandler(IMatchRepository matchRepository, ITeamRepository teamRepository)
    : IRequestHandler<ListMatchesQuery, List<MatchDto>>
{
    public async Task<List<MatchDto>> Handle(ListMatchesQuery request, CancellationToken cancellationToken)
    {
        var matches = request.TeamId is not null
            ? await matchRepository.GetByTeamAsync(request.TeamId, cancellationToken)
            : await matchRepository.GetAllAsync(cancellationToken);

        if (request.Status.HasValue)
            matches = matches.Where(m => m.Status == request.Status.Value).ToList();

        var allTeams = await teamRepository.GetAllAsync(cancellationToken);
        var teamMap = allTeams.ToDictionary(t => t.Id, t => t.Name);

        return matches.Select(m => MatchMapper.ToDto(
            m,
            teamMap.GetValueOrDefault(m.HomeTeamId, m.HomeTeamId),
            teamMap.GetValueOrDefault(m.AwayTeamId, m.AwayTeamId)
        )).ToList();
    }
}
