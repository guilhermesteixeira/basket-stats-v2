namespace BasketStats.Application.Handlers;

using MediatR;
using BasketStats.Application.Commands;
using BasketStats.Domain.Abstractions;
using BasketStats.Domain.Entities;

public class CreateUserCommandHandler(
    IUserRepository userRepository) : IRequestHandler<CreateUserCommand, string>
{
    public async Task<string> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await userRepository.GetByKeycloakIdAsync(request.KeycloakId, cancellationToken);
        if (existing is not null)
            return existing.Id;

        var userId = Guid.NewGuid().ToString();
        var user = User.Create(userId, request.Email, request.Name, request.KeycloakId);
        await userRepository.SaveAsync(user, cancellationToken);

        return user.Id;
    }
}
