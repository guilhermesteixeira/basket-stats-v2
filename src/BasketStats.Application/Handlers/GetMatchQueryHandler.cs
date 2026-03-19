namespace BasketStats.Application.Handlers;

using MediatR;
using BasketStats.Application.DTOs;
using BasketStats.Application.Mapping;
using BasketStats.Application.Queries;
using BasketStats.Domain.Abstractions;

public class GetMatchQueryHandler(IMatchRepository matchRepository) : IRequestHandler<GetMatchQuery, MatchDto?>
{
    public async Task<MatchDto?> Handle(GetMatchQuery request, CancellationToken cancellationToken)
    {
        var match = await matchRepository.GetByIdAsync(request.MatchId, cancellationToken);
        return match is null ? null : MatchMapper.ToDto(match);
    }
}
