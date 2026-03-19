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

    private GetMatchQueryHandler CreateHandler() => new(_matchRepo.Object);

    [Fact]
    public async Task Handle_ExistingMatch_ReturnsMatchDtoWithCorrectScore()
    {
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

        var result = await CreateHandler().Handle(new GetMatchQuery(match.Id.Value), default);

        Assert.NotNull(result);
        Assert.Equal(3, result.HomeScore); // 2 points + 1 free throw
        Assert.Equal(3, result.AwayScore); // 3 points
        Assert.Equal(3, result.Events.Count);
    }

    [Fact]
    public async Task Handle_MatchNotFound_ReturnsNull()
    {
        _matchRepo.Setup(r => r.GetByIdAsync("no-match", default)).ReturnsAsync((DomainMatch?)null);

        var result = await CreateHandler().Handle(new GetMatchQuery("no-match"), default);

        Assert.Null(result);
    }
}
