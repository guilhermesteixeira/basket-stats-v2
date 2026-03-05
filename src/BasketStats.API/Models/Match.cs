namespace BasketStats.API.Models;

/// <summary>
/// Represents a basketball match
/// </summary>
public class Match
{
    public string Id { get; set; } = string.Empty;
    
    public DateTime StartTime { get; set; }
    
    public MatchStatus Status { get; set; }
    
    public List<Team> Teams { get; set; } = new();
    
    public List<Period> Periods { get; set; } = new();
    
    public List<MatchEvent> Events { get; set; } = new();
    
    public Dictionary<string, int> TeamFouls { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Match status enum
/// </summary>
public enum MatchStatus
{
    Scheduled = 0,
    Active = 1,
    Finished = 2
}

/// <summary>
/// Represents a team in a match
/// </summary>
public class Team
{
    public string Id { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    
    public List<Player> Players { get; set; } = new();
    
    public string OwnerId { get; set; } = string.Empty;
}

/// <summary>
/// Represents a player
/// </summary>
public class Player
{
    public string Id { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    
    public int Number { get; set; }
}

/// <summary>
/// Represents a match period (quarter)
/// </summary>
public class Period
{
    public int Number { get; set; }
    
    public DateTime StartTime { get; set; }
    
    public DateTime? EndTime { get; set; }
}

/// <summary>
/// Base class for all match events
/// </summary>
public abstract class MatchEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public DateTime Timestamp { get; set; }
    
    public int PeriodNumber { get; set; }
    
    public int PeriodTimestampSeconds { get; set; }
    
    public string TeamId { get; set; } = string.Empty;
    
    public string PlayerId { get; set; } = string.Empty;
    
    public abstract EventType EventType { get; }
}

/// <summary>
/// Score event (made basket)
/// </summary>
public class ScoreEvent : MatchEvent
{
    public int Points { get; set; } // 2 or 3
    
    public double CoordinateX { get; set; }
    
    public double CoordinateY { get; set; }
    
    public override EventType EventType => EventType.Score;
}

/// <summary>
/// Missed shot event
/// </summary>
public class MissedShotEvent : MatchEvent
{
    public double CoordinateX { get; set; }
    
    public double CoordinateY { get; set; }
    
    public override EventType EventType => EventType.MissedShot;
}

/// <summary>
/// Free throw event
/// </summary>
public class FreeThrowEvent : MatchEvent
{
    public bool Made { get; set; }
    
    public FoulType FoulType { get; set; }
    
    public override EventType EventType => EventType.FreeThrow;
}

/// <summary>
/// Foul event
/// </summary>
public class FoulEvent : MatchEvent
{
    public FoulType FoulType { get; set; }
    
    public string PlayerFouledId { get; set; } = string.Empty;
    
    public bool Flagrant { get; set; }
    
    public override EventType EventType => EventType.Foul;
}

/// <summary>
/// Substitution event
/// </summary>
public class SubstitutionEvent : MatchEvent
{
    public string SubstitutePlayerId { get; set; } = string.Empty;
    
    public override EventType EventType => EventType.Substitution;
}

/// <summary>
/// Event type enum
/// </summary>
public enum EventType
{
    Score = 0,
    MissedShot = 1,
    FreeThrow = 2,
    Foul = 3,
    Substitution = 4
}

/// <summary>
/// Foul type enum
/// </summary>
public enum FoulType
{
    Personal = 0,
    Technical = 1,
    Flagrant = 2
}
