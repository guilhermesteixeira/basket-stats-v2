using Google.Cloud.Firestore;

namespace BasketStats.Infrastructure.Models;

[FirestoreData]
public class MatchDocument
{
    [FirestoreProperty] public string Id { get; set; } = "";
    [FirestoreProperty] public string HomeTeamId { get; set; } = "";
    [FirestoreProperty] public string AwayTeamId { get; set; } = "";
    [FirestoreProperty] public int Status { get; set; }
    [FirestoreProperty] public DateTime CreatedAt { get; set; }
    [FirestoreProperty] public DateTime? StartedAt { get; set; }
    [FirestoreProperty] public DateTime? FinishedAt { get; set; }
    [FirestoreProperty] public List<EventDocument> Events { get; set; } = new();
    [FirestoreProperty] public List<PeriodDocument> Periods { get; set; } = new();
}

[FirestoreData]
public class EventDocument
{
    [FirestoreProperty] public string Id { get; set; } = "";
    [FirestoreProperty] public string TeamId { get; set; } = "";
    [FirestoreProperty] public string PlayerId { get; set; } = "";
    [FirestoreProperty] public int Type { get; set; }
    [FirestoreProperty] public DateTime Timestamp { get; set; }
    [FirestoreProperty] public int PeriodNumber { get; set; }
    [FirestoreProperty] public int PeriodTimestamp { get; set; }
    [FirestoreProperty] public double? CoordinatesX { get; set; }
    [FirestoreProperty] public double? CoordinatesY { get; set; }
    [FirestoreProperty] public int? Points { get; set; }
    [FirestoreProperty] public bool? Made { get; set; }
    [FirestoreProperty] public int? FoulType { get; set; }
    [FirestoreProperty] public string? PlayerFouledId { get; set; }
    [FirestoreProperty] public bool? Flagrant { get; set; }
    [FirestoreProperty] public string? PlayerOutId { get; set; }
}

[FirestoreData]
public class PeriodDocument
{
    [FirestoreProperty] public int Number { get; set; }
    [FirestoreProperty] public DateTime StartTime { get; set; }
    [FirestoreProperty] public DateTime? EndTime { get; set; }
}
