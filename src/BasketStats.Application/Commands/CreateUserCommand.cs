namespace BasketStats.Application.Commands;

using MediatR;

public record CreateUserCommand(string Email, string Name, string KeycloakId) : IRequest<string>;
