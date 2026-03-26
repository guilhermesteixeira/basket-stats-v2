namespace BasketStats.Application.Handlers;

using MediatR;
using BasketStats.Application.DTOs;
using BasketStats.Application.Queries;
using BasketStats.Domain.Abstractions;

public class ListTeamsQueryHandler(ITeamRepository teamRepository) : IRequestHandler<ListTeamsQuery, List<TeamDto>>
{
    public async Task<List<TeamDto>> Handle(ListTeamsQuery request, CancellationToken cancellationToken)
    {
        var teams = await teamRepository.GetAllAsync(cancellationToken);
        return teams.Select(t => new TeamDto
        {
            Id = t.Id,
            Name = t.Name,
            OwnerId = t.OwnerId,
        }).ToList();
    }
}
