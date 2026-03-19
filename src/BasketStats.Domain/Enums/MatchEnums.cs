namespace BasketStats.Domain.Enums;

public enum MatchStatus
{
    Scheduled = 1,
    Active = 2,
    Finished = 3
}

public enum EventType
{
    Score = 1,
    MissedShot = 2,
    FreeThrow = 3,
    Foul = 4,
    Substitution = 5
}

public enum FoulType
{
    Personal = 1,
    Technical = 2,
    Flagrant = 3
}
