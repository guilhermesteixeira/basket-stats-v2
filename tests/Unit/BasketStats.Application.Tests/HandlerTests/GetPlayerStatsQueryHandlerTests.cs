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
        var missed = new MissedShotEvent("home-1", "player-1", new Coordinates(20, 20), PeriodNumber.One, 200);
        var foul = new FoulEvent("home-1", "player-1", Domain.Enums.FoulType.Personal, "player-2", false, PeriodNumber.One, 250);
        var turnover = new TurnoverEvent("home-1", "player-1", PeriodNumber.One, 300);

        match.AddEvent(score);
        match.AddEvent(score2);
        match.AddEvent(missed);
        match.AddEvent(foul);
        match.AddEvent(turnover);

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);

        // Act
        var result = await CreateHandler().Handle(new GetPlayerStatsQuery(match.Id.Value, "player-1"), default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.TotalPoints);  // 2 + 3
        Assert.Equal(1, result.Fouls);
        Assert.Equal(2, result.ShotsMade);    // 2 ScoreEvents
        Assert.Equal(1, result.ShotsMissed);
        Assert.Equal(1, result.Turnovers);
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

    // TC-EVENT-028: Turnover event counts in player stats
    [Fact]
    public async Task Handle_PlayerWithTurnover_CountsTurnover()
    {
        // Arrange
        var match = DomainMatch.Create("home-1", "away-1");
        match.Start();

        var turnover = new TurnoverEvent("home-1", "player-1", PeriodNumber.One, 60);
        match.AddEvent(turnover);

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);

        // Act
        var result = await CreateHandler().Handle(new GetPlayerStatsQuery(match.Id.Value, "player-1"), default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalPoints);
        Assert.Equal(1, result.Turnovers);
    }
}
