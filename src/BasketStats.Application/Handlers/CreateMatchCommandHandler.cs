namespace BasketStats.Application.Handlers;

using MediatR;
using BasketStats.Application.Commands;
using BasketStats.Application.Exceptions;
using BasketStats.Domain.Abstractions;
using BasketStats.Domain.Entities;

public class CreateMatchCommandHandler(
    IMatchRepository matchRepository,
    ITeamRepository teamRepository,
    IUserRepository userRepository) : IRequestHandler<CreateMatchCommand, string>
{
    public async Task<string> Handle(CreateMatchCommand request, CancellationToken cancellationToken)
    {
        var homeTeam = await teamRepository.GetByIdAsync(request.HomeTeamId, cancellationToken);
        if (homeTeam is null)
            throw new NotFoundException($"Home team '{request.HomeTeamId}' not found");

        var awayTeam = await teamRepository.GetByIdAsync(request.AwayTeamId, cancellationToken);
        if (awayTeam is null)
            throw new NotFoundException($"Away team '{request.AwayTeamId}' not found");

        var user = await userRepository.GetByKeycloakIdAsync(request.RequestedByUserId, cancellationToken);
        if (user is null)
            throw new NotFoundException($"User '{request.RequestedByUserId}' not found");

        var match = Match.Create(request.HomeTeamId, request.AwayTeamId);
        await matchRepository.SaveAsync(match, cancellationToken);

        return match.Id.Value;
    }
}
