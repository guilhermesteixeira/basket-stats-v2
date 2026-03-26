namespace BasketStats.Application.Tests.HandlerTests;

using Moq;
using BasketStats.Application.Handlers;
using BasketStats.Application.Queries;
using BasketStats.Domain.Abstractions;
using BasketStats.Domain.Entities;
using BasketStats.Domain.ValueObjects;

public class GetMatchQueryHandlerTests
{
    private readonly Mock<IMatchRepository> _matchRepo = new();
    private readonly Mock<ITeamRepository> _teamRepo = new();

    private GetMatchQueryHandler CreateHandler() => new(_matchRepo.Object, _teamRepo.Object);

    // TC-MATCH-006: Retrieve existing match by ID
    [Fact]
    public async Task Handle_ExistingMatch_ReturnsMatchDtoWithCorrectScore()
    {
        // Arrange
        var match = DomainMatch.Create("home-1", "away-1");
        match.Start();

        var homeScore = new ScoreEvent("home-1", "player-1", 2,
            new Coordinates(50, 50), PeriodNumber.One, 60);
        var awayScore = new ScoreEvent("away-1", "player-2", 3,
            new Coordinates(30, 30), PeriodNumber.One, 90);
        var homeFreeThrow = new FreeThrowEvent("home-1", "player-1", true,
            Domain.Enums.FoulType.Personal, PeriodNumber.One, 120);

        match.AddEvent(homeScore);
        match.AddEvent(awayScore);
        match.AddEvent(homeFreeThrow);

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);

        // Act
        var result = await CreateHandler().Handle(new GetMatchQuery(match.Id.Value), default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.HomeScore); // 2 points + 1 free throw
        Assert.Equal(3, result.AwayScore); // 3 points
        Assert.Equal(3, result.Events.Count);
    }

    // TC-MATCH-007: Fail to retrieve non-existent match (returns null → 404)
    [Fact]
    public async Task Handle_MatchNotFound_ReturnsNull()
    {
        // Arrange
        _matchRepo.Setup(r => r.GetByIdAsync("no-match", default)).ReturnsAsync((DomainMatch?)null);

        // Act
        var result = await CreateHandler().Handle(new GetMatchQuery("no-match"), default);

        // Assert
        Assert.Null(result);
    }

    // TC-EVENT-028: Made free throw counts as 1 point
    [Fact]
    public async Task Handle_MadeFreeThrow_CountsAsOnePoint()
    {
        // Arrange
        var match = DomainMatch.Create("home-1", "away-1");
        match.Start();

        var madeFreeThrow = new FreeThrowEvent("home-1", "player-1", true,
            Domain.Enums.FoulType.Personal, PeriodNumber.One, 60);
        match.AddEvent(madeFreeThrow);

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);

        // Act
        var result = await CreateHandler().Handle(new GetMatchQuery(match.Id.Value), default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.HomeScore);
        Assert.Equal(0, result.AwayScore);
    }

    // TC-EVENT-029: Missed free throw counts as 0 points
    [Fact]
    public async Task Handle_MissedFreeThrow_CountsAsZeroPoints()
    {
        // Arrange
        var match = DomainMatch.Create("home-1", "away-1");
        match.Start();

        var missedFreeThrow = new FreeThrowEvent("home-1", "player-1", false,
            Domain.Enums.FoulType.Personal, PeriodNumber.One, 60);
        match.AddEvent(missedFreeThrow);

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);

        // Act
        var result = await CreateHandler().Handle(new GetMatchQuery(match.Id.Value), default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.HomeScore);
        Assert.Equal(0, result.AwayScore);
    }

    // TC-EVENT-030: Track made vs missed free throws separately in events list
    [Fact]
    public async Task Handle_FreeThrows_TrackedSeparatelyInEvents()
    {
        // Arrange
        var match = DomainMatch.Create("home-1", "away-1");
        match.Start();

        var madeFreeThrow = new FreeThrowEvent("home-1", "player-1", true,
            Domain.Enums.FoulType.Personal, PeriodNumber.One, 60);
        var missedFreeThrow = new FreeThrowEvent("home-1", "player-1", false,
            Domain.Enums.FoulType.Personal, PeriodNumber.One, 90);

        match.AddEvent(madeFreeThrow);
        match.AddEvent(missedFreeThrow);

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);

        // Act
        var result = await CreateHandler().Handle(new GetMatchQuery(match.Id.Value), default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Events.Count);
        Assert.All(result.Events, e => Assert.Equal("FreeThrow", e.Type));
        Assert.Contains(result.Events, e => e.Made == true);
        Assert.Contains(result.Events, e => e.Made == false);
    }
}
