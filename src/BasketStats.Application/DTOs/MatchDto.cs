namespace BasketStats.Application.DTOs;

public class MatchDto
{
    public string Id { get; init; } = string.Empty;
    public MatchTeamDto HomeTeam { get; init; } = null!;
    public MatchTeamDto AwayTeam { get; init; } = null!;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? FinishedAt { get; init; }
    public List<EventDto> Events { get; init; } = new();
    public List<PeriodDto> Periods { get; init; } = new();
    public int HomeScore { get; init; }
    public int AwayScore { get; init; }
}
