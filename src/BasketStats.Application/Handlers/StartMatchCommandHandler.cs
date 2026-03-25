namespace BasketStats.Application.Handlers;

using MediatR;
using BasketStats.Application.Commands;
using BasketStats.Application.Exceptions;
using BasketStats.Domain.Abstractions;

public class StartMatchCommandHandler(
    IMatchRepository matchRepository,
    IUserRepository userRepository) : IRequestHandler<StartMatchCommand>
{
    public async Task Handle(StartMatchCommand request, CancellationToken cancellationToken)
    {
        var match = await matchRepository.GetByIdAsync(request.MatchId, cancellationToken);
        if (match is null)
            throw new NotFoundException($"Match '{request.MatchId}' not found");

        var user = await userRepository.GetByKeycloakIdAsync(request.RequestedByUserId, cancellationToken);
        if (user is null)
            throw new NotFoundException($"User '{request.RequestedByUserId}' not found");

        match.Start();
        await matchRepository.SaveAsync(match, cancellationToken);
    }
}
