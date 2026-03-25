namespace BasketStats.Application.Commands;

using MediatR;

public record CreateTeamCommand(string Name, string RequestedByUserId) : IRequest<string>;
