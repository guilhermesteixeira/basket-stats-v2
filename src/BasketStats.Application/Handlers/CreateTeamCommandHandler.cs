namespace BasketStats.Application.Handlers;

using MediatR;
using BasketStats.Application.Commands;
using BasketStats.Application.Exceptions;
using BasketStats.Domain.Abstractions;
using BasketStats.Domain.Entities;

public class CreateTeamCommandHandler(
    ITeamRepository teamRepository,
    IUserRepository userRepository) : IRequestHandler<CreateTeamCommand, string>
{
    public async Task<string> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByKeycloakIdAsync(request.RequestedByUserId, cancellationToken);
        if (user is null)
            throw new NotFoundException($"User '{request.RequestedByUserId}' not found");

        var teamId = Guid.NewGuid().ToString();
        var team = Team.Create(teamId, request.Name, user.Id);
        await teamRepository.SaveAsync(team, cancellationToken);

        return team.Id;
    }
}
