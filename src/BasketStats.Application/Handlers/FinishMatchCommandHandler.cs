namespace BasketStats.Application.Handlers;

using MediatR;
using BasketStats.Application.Commands;
using BasketStats.Application.Exceptions;
using BasketStats.Domain.Abstractions;

public class FinishMatchCommandHandler(
    IMatchRepository matchRepository) : IRequestHandler<FinishMatchCommand>
{
    public async Task Handle(FinishMatchCommand request, CancellationToken cancellationToken)
    {
        var match = await matchRepository.GetByIdAsync(request.MatchId, cancellationToken);
        if (match is null)
            throw new NotFoundException($"Match '{request.MatchId}' not found");

        match.Finish();
        await matchRepository.SaveAsync(match, cancellationToken);
    }
}
