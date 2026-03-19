using Google.Cloud.Firestore;

namespace BasketStats.Infrastructure.Models;

[FirestoreData]
public class UserDocument
{
    [FirestoreProperty] public string Id { get; set; } = "";
    [FirestoreProperty] public string Email { get; set; } = "";
    [FirestoreProperty] public string Name { get; set; } = "";
    [FirestoreProperty] public string KeycloakId { get; set; } = "";
    [FirestoreProperty] public List<string> Roles { get; set; } = new();
    [FirestoreProperty] public DateTime CreatedAt { get; set; }
}
