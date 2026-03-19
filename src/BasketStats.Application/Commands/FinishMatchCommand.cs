namespace BasketStats.Application.Commands;

using MediatR;

public record FinishMatchCommand(string MatchId, string RequestedByUserId) : IRequest;
