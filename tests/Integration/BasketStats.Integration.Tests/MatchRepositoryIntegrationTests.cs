namespace BasketStats.Integration.Tests;

using BasketStats.Domain.Entities;
using BasketStats.Domain.Enums;
using BasketStats.Domain.ValueObjects;
using BasketStats.Infrastructure.Repositories;

[Trait("Category", "Integration")]
public class MatchRepositoryIntegrationTests : IClassFixture<FirestoreIntegrationFixture>
{
    private readonly FirestoreMatchRepository? _repo;
    private readonly bool _available;

    public MatchRepositoryIntegrationTests(FirestoreIntegrationFixture fixture)
    {
        _available = fixture.IsAvailable;
        if (fixture.IsAvailable)
            _repo = new FirestoreMatchRepository(fixture.FirestoreDb!);
    }

    [SkippableFact]
    public async Task SaveAsync_ThenGetByIdAsync_RoundTrip_ReturnsMatch()
    {
        Skip.IfNot(_available, "Firestore emulator not available");

        var match = Match.Create("home-team", "away-team");

        await _repo!.SaveAsync(match);
        var retrieved = await _repo.GetByIdAsync(match.Id.Value);

        Assert.NotNull(retrieved);
        Assert.Equal(match.Id.Value, retrieved.Id.Value);
        Assert.Equal("home-team", retrieved.HomeTeamId);
        Assert.Equal("away-team", retrieved.AwayTeamId);
        Assert.Equal(MatchStatus.Scheduled, retrieved.Status);
    }

    [SkippableFact]
    public async Task SaveAsync_ActiveMatchWithEvents_PersistsEventsCorrectly()
    {
        Skip.IfNot(_available, "Firestore emulator not available");

        var match = Match.Create("home-team", "away-team");
        match.Start();
        var scoreEvent = new ScoreEvent("home-team", "player-1", 2,
            new Coordinates(50, 50), PeriodNumber.One, 60);
        match.AddEvent(scoreEvent);

        await _repo!.SaveAsync(match);
        var retrieved = await _repo.GetByIdAsync(match.Id.Value);

        Assert.NotNull(retrieved);
        Assert.Equal(MatchStatus.Active, retrieved.Status);
        Assert.Single(retrieved.Events);
        var evt = Assert.IsType<ScoreEvent>(retrieved.Events[0]);
        Assert.Equal(2, evt.Points);
    }

    [SkippableFact]
    public async Task GetAllAsync_ReturnsAllSavedMatches()
    {
        Skip.IfNot(_available, "Firestore emulator not available");

        var match1 = Match.Create("home-1", "away-1");
        var match2 = Match.Create("home-2", "away-2");
        await _repo!.SaveAsync(match1);
        await _repo.SaveAsync(match2);

        var all = await _repo.GetAllAsync();

        Assert.True(all.Count >= 2);
    }

    [SkippableFact]
    public async Task GetByTeamAsync_ReturnsMatchesForTeam()
    {
        Skip.IfNot(_available, "Firestore emulator not available");

        var match = Match.Create("filter-home", "filter-away");
        await _repo!.SaveAsync(match);

        var results = await _repo.GetByTeamAsync("filter-home");

        Assert.Contains(results, m => m.Id.Value == match.Id.Value);
    }

    [SkippableFact]
    public async Task DeleteAsync_RemovesMatch()
    {
        Skip.IfNot(_available, "Firestore emulator not available");

        var match = Match.Create("del-home", "del-away");
        await _repo!.SaveAsync(match);

        await _repo.DeleteAsync(match.Id.Value);
        var result = await _repo.GetByIdAsync(match.Id.Value);

        Assert.Null(result);
    }
}
