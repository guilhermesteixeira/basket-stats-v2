using BasketStats.Domain.Abstractions;
using BasketStats.Domain.Entities;
using BasketStats.Infrastructure.Mapping;
using BasketStats.Infrastructure.Models;
using Google.Cloud.Firestore;

namespace BasketStats.Infrastructure.Repositories;

public class FirestoreTeamRepository(FirestoreDb db) : ITeamRepository
{
    private const string Collection = "teams";

    public async Task<Team?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var doc = await db.Collection(Collection).Document(id).GetSnapshotAsync(ct);
        if (!doc.Exists) return null;
        return TeamFirestoreMapper.ToDomain(doc.ConvertTo<TeamDocument>());
    }

    public async Task<List<Team>> GetByOwnerAsync(string ownerId, CancellationToken ct = default)
    {
        var all = await GetAllAsync(ct);
        return all.Where(t => t.OwnerId == ownerId).ToList();
    }

    public async Task SaveAsync(Team team, CancellationToken ct = default)
    {
        var doc = TeamFirestoreMapper.ToDocument(team);
        await db.Collection(Collection).Document(team.Id).SetAsync(doc, null, ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await db.Collection(Collection).Document(id).DeleteAsync(cancellationToken: ct);
    }

    private async Task<List<Team>> GetAllAsync(CancellationToken ct = default)
    {
        var snapshot = await db.Collection(Collection).GetSnapshotAsync(ct);
        return snapshot.Documents
            .Select(d => TeamFirestoreMapper.ToDomain(d.ConvertTo<TeamDocument>()))
            .ToList();
    }
}
