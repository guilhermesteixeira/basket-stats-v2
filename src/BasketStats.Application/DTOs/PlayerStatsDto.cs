namespace BasketStats.Application.DTOs;

public class PlayerStatsDto
{
    public string PlayerId { get; init; } = string.Empty;
    public string MatchId { get; init; } = string.Empty;
    public int TotalPoints { get; init; }
    public int FreeThrowsMade { get; init; }
    public int FreeThrowsMissed { get; init; }
    public int Fouls { get; init; }
    public int ShotsMade { get; init; }
    public int ShotsMissed { get; init; }
}
