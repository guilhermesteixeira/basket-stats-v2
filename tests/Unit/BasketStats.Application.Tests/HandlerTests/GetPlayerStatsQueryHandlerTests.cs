namespace BasketStats.Application.Tests.HandlerTests;

using Moq;
using BasketStats.Application.Handlers;
using BasketStats.Application.Queries;
using BasketStats.Domain.Abstractions;
using BasketStats.Domain.Entities;
using BasketStats.Domain.ValueObjects;

public class GetPlayerStatsQueryHandlerTests
{
    private readonly Mock<IMatchRepository> _matchRepo = new();

    private GetPlayerStatsQueryHandler CreateHandler() => new(_matchRepo.Object);

    [Fact]
    public async Task Handle_ExistingPlayerWithEvents_ReturnsCorrectStats()
    {
        // Arrange
        var match = DomainMatch.Create("home-1", "away-1");
        match.Start();

        var score = new ScoreEvent("home-1", "player-1", 2, new Coordinates(50, 50), PeriodNumber.One, 60);
        var score2 = new ScoreEvent("home-1", "player-1", 3, new Coordinates(80, 40), PeriodNumber.One, 120);
        var ftMade = new FreeThrowEvent("home-1", "player-1", true, Domain.Enums.FoulType.Personal, PeriodNumber.One, 130);
        var ftMissed = new FreeThrowEvent("home-1", "player-1", false, Domain.Enums.FoulType.Personal, PeriodNumber.One, 140);
        var missed = new MissedShotEvent("home-1", "player-1", new Coordinates(20, 20), PeriodNumber.One, 200);
        var foul = new FoulEvent("home-1", "player-1", Domain.Enums.FoulType.Personal, "player-2", false, PeriodNumber.One, 250);

        match.AddEvent(score);
        match.AddEvent(score2);
        match.AddEvent(ftMade);
        match.AddEvent(ftMissed);
        match.AddEvent(missed);
        match.AddEvent(foul);

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);

        // Act
        var result = await CreateHandler().Handle(new GetPlayerStatsQuery(match.Id.Value, "player-1"), default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(6, result.TotalPoints);  // 2 + 3 + 1
        Assert.Equal(1, result.FreeThrowsMade);
        Assert.Equal(1, result.FreeThrowsMissed);
        Assert.Equal(1, result.Fouls);
        Assert.Equal(2, result.ShotsMade);    // 2 ScoreEvents
        Assert.Equal(1, result.ShotsMissed);
    }

    [Fact]
    public async Task Handle_MatchNotFound_ReturnsNull()
    {
        // Arrange
        _matchRepo.Setup(r => r.GetByIdAsync("no-match", default)).ReturnsAsync((DomainMatch?)null);

        // Act
        var result = await CreateHandler().Handle(new GetPlayerStatsQuery("no-match", "player-1"), default);

        // Assert
        Assert.Null(result);
    }

    // TC-EVENT-028: Made free throw counts as 1 point for player stats
    [Fact]
    public async Task Handle_PlayerWithMadeFreeThrow_CountsOnePoint()
    {
        // Arrange
        var match = DomainMatch.Create("home-1", "away-1");
        match.Start();

        var ftMade = new FreeThrowEvent("home-1", "player-1", true,
            Domain.Enums.FoulType.Personal, PeriodNumber.One, 60);
        match.AddEvent(ftMade);

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);

        // Act
        var result = await CreateHandler().Handle(new GetPlayerStatsQuery(match.Id.Value, "player-1"), default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalPoints);
        Assert.Equal(1, result.FreeThrowsMade);
        Assert.Equal(0, result.FreeThrowsMissed);
    }

    // TC-EVENT-029: Missed free throw counts as 0 points for player stats
    [Fact]
    public async Task Handle_PlayerWithMissedFreeThrow_CountsZeroPoints()
    {
        // Arrange
        var match = DomainMatch.Create("home-1", "away-1");
        match.Start();

        var ftMissed = new FreeThrowEvent("home-1", "player-1", false,
            Domain.Enums.FoulType.Personal, PeriodNumber.One, 60);
        match.AddEvent(ftMissed);

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);

        // Act
        var result = await CreateHandler().Handle(new GetPlayerStatsQuery(match.Id.Value, "player-1"), default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalPoints);
        Assert.Equal(0, result.FreeThrowsMade);
        Assert.Equal(1, result.FreeThrowsMissed);
    }
}
