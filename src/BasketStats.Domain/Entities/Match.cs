namespace BasketStats.Domain.Entities;

using Abstractions;
using Enums;
using ValueObjects;

public class Match : Entity
{
    private readonly List<Event> _events = new();
    private readonly List<Period> _periods = new();
    private readonly List<PlayerInfo> _homePlayers = new();
    private readonly List<PlayerInfo> _awayPlayers = new();

    public MatchId Id { get; private set; }
    public string HomeTeamId { get; private set; }
    public string AwayTeamId { get; private set; }
    public MatchStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    public IReadOnlyList<Event> Events => _events.AsReadOnly();
    public IReadOnlyList<Period> Periods => _periods.AsReadOnly();
    public IReadOnlyList<PlayerInfo> HomePlayers => _homePlayers.AsReadOnly();
    public IReadOnlyList<PlayerInfo> AwayPlayers => _awayPlayers.AsReadOnly();

    private Match()
    {
    }

    public static Match Create(string homeTeamId, string awayTeamId,
        List<PlayerInfo>? homePlayers = null, List<PlayerInfo>? awayPlayers = null)
    {
        if (string.IsNullOrWhiteSpace(homeTeamId))
            throw new ArgumentException("Home team ID cannot be empty", nameof(homeTeamId));

        if (string.IsNullOrWhiteSpace(awayTeamId))
            throw new ArgumentException("Away team ID cannot be empty", nameof(awayTeamId));

        if (homeTeamId == awayTeamId)
            throw new InvalidOperationException("Home and away teams must be different");

        var match = new Match
        {
            Id = MatchId.CreateNew(),
            HomeTeamId = homeTeamId,
            AwayTeamId = awayTeamId,
            Status = MatchStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        // Initialize 4 periods
        for (int i = 1; i <= 4; i++)
        {
            match._periods.Add(new Period((PeriodNumber)i, DateTime.UtcNow));
        }

        if (homePlayers is not null)
            match._homePlayers.AddRange(homePlayers);
        if (awayPlayers is not null)
            match._awayPlayers.AddRange(awayPlayers);

        return match;
    }

    public void Start()
    {
        if (Status != MatchStatus.Scheduled)
            throw new InvalidOperationException("Can only start a scheduled match");

        Status = MatchStatus.Active;
        StartedAt = DateTime.UtcNow;
    }

    public void Finish()
    {
        if (Status != MatchStatus.Active)
            throw new InvalidOperationException("Can only finish an active match");

        Status = MatchStatus.Finished;
        FinishedAt = DateTime.UtcNow;
    }

    public void AddEvent(Event @event)
    {
        if (Status != MatchStatus.Active)
            throw new InvalidOperationException("Cannot add event to non-active match");

        _events.Add(@event);
    }

    public int GetTeamFoulCount(string teamId, PeriodNumber period)
    {
        return _events
            .Where(e => e.Type == EventType.Foul && e.TeamId == teamId && e.PeriodNumber == period)
            .Count();
    }
}
