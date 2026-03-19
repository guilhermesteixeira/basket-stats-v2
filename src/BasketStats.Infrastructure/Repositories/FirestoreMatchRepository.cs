using BasketStats.Domain.Abstractions;
using BasketStats.Domain.Entities;
using BasketStats.Infrastructure.Mapping;
using BasketStats.Infrastructure.Models;
using Google.Cloud.Firestore;

namespace BasketStats.Infrastructure.Repositories;

public class FirestoreMatchRepository(FirestoreDb db) : IMatchRepository
{
    private const string Collection = "matches";

    public async Task<Match?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var doc = await db.Collection(Collection).Document(id).GetSnapshotAsync(ct);
        if (!doc.Exists) return null;
        return MatchFirestoreMapper.ToDomain(doc.ConvertTo<MatchDocument>());
    }

    public async Task<List<Match>> GetAllAsync(CancellationToken ct = default)
    {
        var snapshot = await db.Collection(Collection).GetSnapshotAsync(ct);
        return snapshot.Documents
            .Select(d => MatchFirestoreMapper.ToDomain(d.ConvertTo<MatchDocument>()))
            .ToList();
    }

    public async Task<List<Match>> GetByTeamAsync(string teamId, CancellationToken ct = default)
    {
        var all = await GetAllAsync(ct);
        return all.Where(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId).ToList();
    }

    public async Task SaveAsync(Match match, CancellationToken ct = default)
    {
        var doc = MatchFirestoreMapper.ToDocument(match);
        await db.Collection(Collection).Document(match.Id.Value).SetAsync(doc, null, ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await db.Collection(Collection).Document(id).DeleteAsync(cancellationToken: ct);
    }
}
