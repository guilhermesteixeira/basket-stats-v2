namespace BasketStats.Application.Commands;

using MediatR;

public record CreateMatchCommand(
    string HomeTeamId,
    string AwayTeamId,
    string RequestedByUserId,
    List<PlayerInput>? HomePlayers = null,
    List<PlayerInput>? AwayPlayers = null) : IRequest<string>;

public record PlayerInput(string Name, int Number);
