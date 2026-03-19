namespace BasketStats.Application.Tests.HandlerTests;

using Moq;
using BasketStats.Application.Handlers;
using BasketStats.Application.Queries;
using BasketStats.Domain.Abstractions;
using BasketStats.Domain.Enums;

public class ListMatchesQueryHandlerTests
{
    private readonly Mock<IMatchRepository> _matchRepo = new();

    private ListMatchesQueryHandler CreateHandler() => new(_matchRepo.Object);

    [Fact]
    public async Task Handle_NoFilters_ReturnsAllMatches()
    {
        var matches = new List<DomainMatch>
        {
            DomainMatch.Create("home-1", "away-1"),
            DomainMatch.Create("home-2", "away-2")
        };
        _matchRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(matches);

        var result = await CreateHandler().Handle(new ListMatchesQuery(), default);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_TeamIdFilter_CallsGetByTeam()
    {
        var match = DomainMatch.Create("home-1", "away-1");
        _matchRepo.Setup(r => r.GetByTeamAsync("home-1", default)).ReturnsAsync(new List<DomainMatch> { match });

        var result = await CreateHandler().Handle(new ListMatchesQuery(TeamId: "home-1"), default);

        Assert.Single(result);
        _matchRepo.Verify(r => r.GetByTeamAsync("home-1", default), Times.Once);
    }

    [Fact]
    public async Task Handle_StatusFilter_ReturnsOnlyMatchingMatches()
    {
        var scheduled = DomainMatch.Create("home-1", "away-1");
        var active = DomainMatch.Create("home-2", "away-2");
        active.Start();

        _matchRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<DomainMatch> { scheduled, active });

        var result = await CreateHandler().Handle(new ListMatchesQuery(Status: MatchStatus.Active), default);

        Assert.Single(result);
        Assert.Equal("Active", result[0].Status);
    }
}
