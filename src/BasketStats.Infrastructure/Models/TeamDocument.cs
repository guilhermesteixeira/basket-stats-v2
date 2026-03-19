using Google.Cloud.Firestore;

namespace BasketStats.Infrastructure.Models;

[FirestoreData]
public class TeamDocument
{
    [FirestoreProperty] public string Id { get; set; } = "";
    [FirestoreProperty] public string Name { get; set; } = "";
    [FirestoreProperty] public string OwnerId { get; set; } = "";
    [FirestoreProperty] public DateTime CreatedAt { get; set; }
}
