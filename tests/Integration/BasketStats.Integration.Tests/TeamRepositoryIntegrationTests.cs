namespace BasketStats.Integration.Tests;

using BasketStats.Domain.Entities;
using BasketStats.Infrastructure.Repositories;

[Trait("Category", "Integration")]
public class TeamRepositoryIntegrationTests : IClassFixture<FirestoreIntegrationFixture>
{
    private readonly FirestoreTeamRepository? _repo;
    private readonly bool _available;

    public TeamRepositoryIntegrationTests(FirestoreIntegrationFixture fixture)
    {
        _available = fixture.IsAvailable;
        if (fixture.IsAvailable)
            _repo = new FirestoreTeamRepository(fixture.FirestoreDb!);
    }

    [SkippableFact]
    public async Task SaveAsync_ThenGetByIdAsync_RoundTrip_ReturnsTeam()
    {
        Skip.IfNot(_available, "Firestore emulator not available");

        var team = Team.Create("team-id-1", "Lakers", "owner-1");

        await _repo!.SaveAsync(team);
        var retrieved = await _repo.GetByIdAsync(team.Id);

        Assert.NotNull(retrieved);
        Assert.Equal(team.Id, retrieved.Id);
        Assert.Equal("Lakers", retrieved.Name);
        Assert.Equal("owner-1", retrieved.OwnerId);
    }

    [SkippableFact]
    public async Task GetByOwnerAsync_ReturnsTeamsForOwner()
    {
        Skip.IfNot(_available, "Firestore emulator not available");

        var team1 = Team.Create("owner-team-1", "Celtics", "owner-abc");
        var team2 = Team.Create("owner-team-2", "Bulls", "owner-abc");
        var other = Team.Create("other-team-1", "Heat", "owner-xyz");
        await _repo!.SaveAsync(team1);
        await _repo.SaveAsync(team2);
        await _repo.SaveAsync(other);

        var results = await _repo.GetByOwnerAsync("owner-abc");

        Assert.Contains(results, t => t.Id == team1.Id);
        Assert.Contains(results, t => t.Id == team2.Id);
        Assert.DoesNotContain(results, t => t.Id == other.Id);
    }

    [SkippableFact]
    public async Task DeleteAsync_RemovesTeam()
    {
        Skip.IfNot(_available, "Firestore emulator not available");

        var team = Team.Create("del-team-1", "Spurs", "owner-del");
        await _repo!.SaveAsync(team);

        await _repo.DeleteAsync(team.Id);
        var result = await _repo.GetByIdAsync(team.Id);

        Assert.Null(result);
    }
}
