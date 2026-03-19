namespace BasketStats.Application.Tests.HandlerTests;

using Moq;
using BasketStats.Application.Commands;
using BasketStats.Application.Exceptions;
using BasketStats.Application.Handlers;
using BasketStats.Domain.Abstractions;
using BasketStats.Domain.Entities;
using BasketStats.Domain.Enums;
using BasketStats.Domain.ValueObjects;

using DomainMatch = BasketStats.Domain.Entities.Match;

public class AddEventCommandHandlerTests
{
    private readonly Mock<IMatchRepository> _matchRepo = new();
    private readonly Mock<ITeamRepository> _teamRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();

    private AddEventCommandHandler CreateHandler() =>
        new(_matchRepo.Object, _teamRepo.Object, _userRepo.Object);

    private static DomainMatch CreateActiveMatch(string homeTeamId = "home-1", string awayTeamId = "away-1")
    {
        var match = DomainMatch.Create(homeTeamId, awayTeamId);
        match.Start();
        return match;
    }

    private AddEventCommand BuildScoreCommand(string matchId, string teamId, string userId) => new()
    {
        MatchId = matchId,
        TeamId = teamId,
        PlayerId = "player-1",
        Type = EventType.Score,
        PeriodNumber = PeriodNumber.One,
        PeriodTimestamp = 60,
        RequestedByUserId = userId,
        Points = 2,
        CoordinatesX = 50,
        CoordinatesY = 50
    };

    [Fact]
    public async Task Handle_HomeOwnerAddsScoreForHomeTeam_AddsEvent()
    {
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "user-1");
        var awayTeam = Team.Create("away-1", "Away", "owner-2");
        var user = User.Create("user-1", "u@t.com", "User", "kc-1");

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);
        _userRepo.Setup(r => r.GetByIdAsync("user-1", default)).ReturnsAsync(user);
        _teamRepo.Setup(r => r.GetByIdAsync("home-1", default)).ReturnsAsync(homeTeam);
        _teamRepo.Setup(r => r.GetByIdAsync("away-1", default)).ReturnsAsync(awayTeam);
        _matchRepo.Setup(r => r.SaveAsync(match, default)).Returns(Task.CompletedTask);

        var command = BuildScoreCommand(match.Id.Value, "home-1", "user-1");
        var result = await CreateHandler().Handle(command, default);

        Assert.NotEmpty(result);
        Assert.Single(match.Events);
    }

    [Fact]
    public async Task Handle_HomeOwnerAddsFoulForHomeTeam_AddsEvent()
    {
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "user-1");
        var awayTeam = Team.Create("away-1", "Away", "owner-2");
        var user = User.Create("user-1", "u@t.com", "User", "kc-1");

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);
        _userRepo.Setup(r => r.GetByIdAsync("user-1", default)).ReturnsAsync(user);
        _teamRepo.Setup(r => r.GetByIdAsync("home-1", default)).ReturnsAsync(homeTeam);
        _teamRepo.Setup(r => r.GetByIdAsync("away-1", default)).ReturnsAsync(awayTeam);
        _matchRepo.Setup(r => r.SaveAsync(match, default)).Returns(Task.CompletedTask);

        var command = new AddEventCommand
        {
            MatchId = match.Id.Value,
            TeamId = "home-1",
            PlayerId = "player-1",
            Type = EventType.Foul,
            PeriodNumber = PeriodNumber.One,
            PeriodTimestamp = 60,
            RequestedByUserId = "user-1",
            FoulType = FoulType.Personal,
            PlayerFouledId = "player-2",
            Flagrant = false
        };

        var result = await CreateHandler().Handle(command, default);

        Assert.NotEmpty(result);
        Assert.Single(match.Events);
    }

    [Fact]
    public async Task Handle_AwayOwnerAddsScoreForHomeTeam_OpponentTrackingAllowed()
    {
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "owner-home");
        var awayTeam = Team.Create("away-1", "Away", "user-away");
        var user = User.Create("user-away", "away@t.com", "Away User", "kc-away");

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);
        _userRepo.Setup(r => r.GetByIdAsync("user-away", default)).ReturnsAsync(user);
        _teamRepo.Setup(r => r.GetByIdAsync("home-1", default)).ReturnsAsync(homeTeam);
        _teamRepo.Setup(r => r.GetByIdAsync("away-1", default)).ReturnsAsync(awayTeam);
        _matchRepo.Setup(r => r.SaveAsync(match, default)).Returns(Task.CompletedTask);

        var command = BuildScoreCommand(match.Id.Value, "home-1", "user-away");
        var result = await CreateHandler().Handle(command, default);

        Assert.NotEmpty(result);
        Assert.Single(match.Events);
    }

    [Fact]
    public async Task Handle_UnrelatedUser_ThrowsForbiddenException()
    {
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "owner-home");
        var awayTeam = Team.Create("away-1", "Away", "owner-away");
        var user = User.Create("user-x", "x@t.com", "Other User", "kc-x");

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);
        _userRepo.Setup(r => r.GetByIdAsync("user-x", default)).ReturnsAsync(user);
        _teamRepo.Setup(r => r.GetByIdAsync("home-1", default)).ReturnsAsync(homeTeam);
        _teamRepo.Setup(r => r.GetByIdAsync("away-1", default)).ReturnsAsync(awayTeam);

        var command = BuildScoreCommand(match.Id.Value, "home-1", "user-x");

        await Assert.ThrowsAsync<ForbiddenException>(() => CreateHandler().Handle(command, default));
    }

    [Fact]
    public async Task Handle_AwayOwnerAddsFoulForHomeTeam_ThrowsForbiddenException()
    {
        var match = CreateActiveMatch("home-1", "away-1");
        var homeTeam = Team.Create("home-1", "Home", "owner-home");
        var awayTeam = Team.Create("away-1", "Away", "user-away");
        var user = User.Create("user-away", "away@t.com", "Away User", "kc-away");

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);
        _userRepo.Setup(r => r.GetByIdAsync("user-away", default)).ReturnsAsync(user);
        _teamRepo.Setup(r => r.GetByIdAsync("home-1", default)).ReturnsAsync(homeTeam);
        _teamRepo.Setup(r => r.GetByIdAsync("away-1", default)).ReturnsAsync(awayTeam);

        var command = new AddEventCommand
        {
            MatchId = match.Id.Value,
            TeamId = "home-1",
            PlayerId = "player-1",
            Type = EventType.Foul,
            PeriodNumber = PeriodNumber.One,
            PeriodTimestamp = 60,
            RequestedByUserId = "user-away",
            FoulType = FoulType.Personal,
            PlayerFouledId = "player-2",
            Flagrant = false
        };

        await Assert.ThrowsAsync<ForbiddenException>(() => CreateHandler().Handle(command, default));
    }

    [Fact]
    public async Task Handle_MatchNotFound_ThrowsNotFoundException()
    {
        _matchRepo.Setup(r => r.GetByIdAsync("no-match", default)).ReturnsAsync((DomainMatch?)null);

        var command = BuildScoreCommand("no-match", "home-1", "user-1");

        await Assert.ThrowsAsync<NotFoundException>(() => CreateHandler().Handle(command, default));
    }
}
