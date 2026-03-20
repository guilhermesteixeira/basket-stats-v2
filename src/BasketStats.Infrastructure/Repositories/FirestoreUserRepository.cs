using BasketStats.Domain.Abstractions;
using BasketStats.Domain.Entities;
using BasketStats.Infrastructure.Mapping;
using BasketStats.Infrastructure.Models;
using Google.Cloud.Firestore;

namespace BasketStats.Infrastructure.Repositories;

public class FirestoreUserRepository(FirestoreDb db) : IUserRepository
{
    private const string Collection = "users";

    public async Task<User?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var doc = await db.Collection(Collection).Document(id).GetSnapshotAsync(ct);
        if (!doc.Exists) return null;
        return UserFirestoreMapper.ToDomain(doc.ConvertTo<UserDocument>());
    }

    public async Task<User?> GetByKeycloakIdAsync(string keycloakId, CancellationToken ct = default)
    {
        var snapshot = await db.Collection(Collection)
            .WhereEqualTo("KeycloakId", keycloakId)
            .GetSnapshotAsync(ct);
        var doc = snapshot.Documents.FirstOrDefault();
        return doc is null ? null : UserFirestoreMapper.ToDomain(doc.ConvertTo<UserDocument>());
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var snapshot = await db.Collection(Collection)
            .WhereEqualTo("Email", email)
            .GetSnapshotAsync(ct);
        var doc = snapshot.Documents.FirstOrDefault();
        return doc is null ? null : UserFirestoreMapper.ToDomain(doc.ConvertTo<UserDocument>());
    }

    public async Task SaveAsync(User user, CancellationToken ct = default)
    {
        var doc = UserFirestoreMapper.ToDocument(user);
        await db.Collection(Collection).Document(user.Id).SetAsync(doc, null, ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await db.Collection(Collection).Document(id).DeleteAsync(cancellationToken: ct);
    }
}
