namespace BasketStats.Application.Commands;

using MediatR;

public record CreateMatchCommand(string HomeTeamId, string AwayTeamId, string RequestedByUserId) : IRequest<string>;
