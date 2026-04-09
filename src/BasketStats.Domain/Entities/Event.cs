namespace BasketStats.Domain.Entities;

using Abstractions;
using Enums;
using ValueObjects;

public abstract class Event : Entity
{
    public EventId Id { get; protected set; }
    public string TeamId { get; protected set; }
    public string PlayerId { get; protected set; }
    public EventType Type { get; protected set; }
    public DateTime Timestamp { get; protected set; }
    public PeriodNumber PeriodNumber { get; protected set; }
    public int PeriodTimestamp { get; protected set; } // seconds within period

    protected Event()
    {
    }

    protected Event(string teamId, string playerId, EventType type, PeriodNumber periodNumber, int periodTimestamp)
    {
        if (string.IsNullOrWhiteSpace(teamId))
            throw new ArgumentException("Team ID cannot be empty", nameof(teamId));

        if (string.IsNullOrWhiteSpace(playerId))
            throw new ArgumentException("Player ID cannot be empty", nameof(playerId));

        if (periodTimestamp < 0 || periodTimestamp > 600)
            throw new ArgumentException("Period timestamp must be between 0 and 600 seconds", nameof(periodTimestamp));

        Id = EventId.CreateNew();
        TeamId = teamId;
        PlayerId = playerId;
        Type = type;
        PeriodNumber = periodNumber;
        PeriodTimestamp = periodTimestamp;
        Timestamp = DateTime.UtcNow;
    }
}

public class ScoreEvent : Event
{
    public int Points { get; }
    public Coordinates Coordinates { get; }

    public ScoreEvent(string teamId, string playerId, int points, Coordinates coordinates, PeriodNumber periodNumber, int periodTimestamp)
        : base(teamId, playerId, EventType.Score, periodNumber, periodTimestamp)
    {
        if (points != 2 && points != 3)
            throw new ArgumentException("Points must be 2 or 3", nameof(points));

        Points = points;
        Coordinates = coordinates ?? throw new ArgumentNullException(nameof(coordinates));
    }
}

public class MissedShotEvent : Event
{
    public Coordinates Coordinates { get; }

    public MissedShotEvent(string teamId, string playerId, Coordinates coordinates, PeriodNumber periodNumber, int periodTimestamp)
        : base(teamId, playerId, EventType.MissedShot, periodNumber, periodTimestamp)
    {
        Coordinates = coordinates ?? throw new ArgumentNullException(nameof(coordinates));
    }
}

public class TurnoverEvent : Event
{
    public TurnoverEvent(string teamId, string playerId, PeriodNumber periodNumber, int periodTimestamp)
        : base(teamId, playerId, EventType.Turnover, periodNumber, periodTimestamp)
    {
    }
}

public class FoulEvent : Event
{
    public FoulType FoulType { get; }
    public string PlayerFouledId { get; }
    public bool Flagrant { get; }

    public FoulEvent(string teamId, string playerId, FoulType foulType, string playerFouledId, bool flagrant, PeriodNumber periodNumber, int periodTimestamp)
        : base(teamId, playerId, EventType.Foul, periodNumber, periodTimestamp)
    {
        if (string.IsNullOrWhiteSpace(playerFouledId))
            throw new ArgumentException("Fouled player ID cannot be empty", nameof(playerFouledId));

        FoulType = foulType;
        PlayerFouledId = playerFouledId;
        Flagrant = flagrant;
    }
}

public class SubstitutionEvent : Event
{
    public string PlayerOutId { get; }

    public SubstitutionEvent(string teamId, string playerInId, string playerOutId, PeriodNumber periodNumber, int periodTimestamp)
        : base(teamId, playerInId, EventType.Substitution, periodNumber, periodTimestamp)
    {
        if (string.IsNullOrWhiteSpace(playerOutId))
            throw new ArgumentException("Player out ID cannot be empty", nameof(playerOutId));

        PlayerOutId = playerOutId;
    }
}
