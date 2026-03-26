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

        match.AddEvent(homeScore);
        match.AddEvent(awayScore);

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);

        // Act
        var result = await CreateHandler().Handle(new GetMatchQuery(match.Id.Value), default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.HomeScore); // 2 points
        Assert.Equal(3, result.AwayScore); // 3 points
        Assert.Equal(2, result.Events.Count);
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

    // TC-EVENT-028: Turnover event tracked in match events
    [Fact]
    public async Task Handle_TurnoverEvent_TrackedInEvents()
    {
        // Arrange
        var match = DomainMatch.Create("home-1", "away-1");
        match.Start();

        var turnover = new TurnoverEvent("home-1", "player-1", PeriodNumber.One, 60);
        match.AddEvent(turnover);

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);

        // Act
        var result = await CreateHandler().Handle(new GetMatchQuery(match.Id.Value), default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.HomeScore);
        Assert.Equal(0, result.AwayScore);
        Assert.Single(result.Events);
        Assert.Equal("Turnover", result.Events[0].Type);
    }
}
