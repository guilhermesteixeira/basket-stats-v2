namespace BasketStats.Application.Handlers;

using MediatR;
using BasketStats.Application.DTOs;
using BasketStats.Application.Mapping;
using BasketStats.Application.Queries;
using BasketStats.Domain.Abstractions;

public class ListMatchesQueryHandler(IMatchRepository matchRepository) : IRequestHandler<ListMatchesQuery, List<MatchDto>>
{
    public async Task<List<MatchDto>> Handle(ListMatchesQuery request, CancellationToken cancellationToken)
    {
        var matches = request.TeamId is not null
            ? await matchRepository.GetByTeamAsync(request.TeamId, cancellationToken)
            : await matchRepository.GetAllAsync(cancellationToken);

        if (request.Status.HasValue)
            matches = matches.Where(m => m.Status == request.Status.Value).ToList();

        return matches.Select(MatchMapper.ToDto).ToList();
    }
}
