namespace BasketStats.API.Requests;

public record CreateMatchRequest(
    string HomeTeamId,
    string AwayTeamId,
    List<PlayerInfoRequest>? HomePlayers = null,
    List<PlayerInfoRequest>? AwayPlayers = null);

public record PlayerInfoRequest(string Name, int Number);
