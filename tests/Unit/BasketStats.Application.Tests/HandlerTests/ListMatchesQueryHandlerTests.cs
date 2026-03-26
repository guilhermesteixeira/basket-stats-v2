namespace BasketStats.Application.Tests.HandlerTests;

using Moq;
using BasketStats.Application.Handlers;
using BasketStats.Application.Queries;
using BasketStats.Domain.Abstractions;
using BasketStats.Domain.Enums;

public class ListMatchesQueryHandlerTests
{
    private readonly Mock<IMatchRepository> _matchRepo = new();
    private readonly Mock<ITeamRepository> _teamRepo = new();

    private ListMatchesQueryHandler CreateHandler() => new(_matchRepo.Object, _teamRepo.Object);

    [Fact]
    public async Task Handle_NoFilters_ReturnsAllMatches()
    {
        // Arrange
        var matches = new List<DomainMatch>
        {
            DomainMatch.Create("home-1", "away-1"),
            DomainMatch.Create("home-2", "away-2")
        };
        _matchRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(matches);
        _teamRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<BasketStats.Domain.Entities.Team>());

        // Act
        var result = await CreateHandler().Handle(new ListMatchesQuery(), default);

        // Assert
        Assert.Equal(2, result.Count);
    }

    // TC-MATCH-009: Filter matches by team
    [Fact]
    public async Task Handle_TeamIdFilter_CallsGetByTeam()
    {
        // Arrange
        var match = DomainMatch.Create("home-1", "away-1");
        _matchRepo.Setup(r => r.GetByTeamAsync("home-1", default)).ReturnsAsync(new List<DomainMatch> { match });
        _teamRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<BasketStats.Domain.Entities.Team>());

        // Act
        var result = await CreateHandler().Handle(new ListMatchesQuery(TeamId: "home-1"), default);

        // Assert
        Assert.Single(result);
        _matchRepo.Verify(r => r.GetByTeamAsync("home-1", default), Times.Once);
    }

    // TC-MATCH-010: Filter matches by status
    [Fact]
    public async Task Handle_StatusFilter_ReturnsOnlyMatchingMatches()
    {
        // Arrange
        var scheduled = DomainMatch.Create("home-1", "away-1");
        var active = DomainMatch.Create("home-2", "away-2");
        active.Start();

        _matchRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<DomainMatch> { scheduled, active });
        _teamRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<BasketStats.Domain.Entities.Team>());

        // Act
        var result = await CreateHandler().Handle(new ListMatchesQuery(Status: MatchStatus.Active), default);

        // Assert
        Assert.Single(result);
        Assert.Equal("Active", result[0].Status);
    }
}
