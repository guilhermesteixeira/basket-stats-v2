namespace BasketStats.Application.Commands;

using MediatR;

public record StartMatchCommand(string MatchId, string RequestedByUserId) : IRequest;
