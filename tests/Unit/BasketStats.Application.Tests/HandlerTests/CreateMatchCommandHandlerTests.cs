namespace BasketStats.Application.Tests.HandlerTests;

using Moq;
using BasketStats.Application.Commands;
using BasketStats.Application.Exceptions;
using BasketStats.Application.Handlers;
using BasketStats.Domain.Abstractions;
using BasketStats.Domain.Entities;

public class CreateMatchCommandHandlerTests
{
    private readonly Mock<IMatchRepository> _matchRepo = new();
    private readonly Mock<ITeamRepository> _teamRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();

    private CreateMatchCommandHandler CreateHandler() =>
        new(_matchRepo.Object, _teamRepo.Object, _userRepo.Object);

    // TC-MATCH-001: Successfully create match with valid data
    [Fact]
    public async Task Handle_ValidCommand_CreatesMatchAndReturnsId()
    {
        // Arrange
        var homeTeam = Team.Create("home-1", "Home Team", "owner-1");
        var awayTeam = Team.Create("away-1", "Away Team", "owner-2");
        var user = User.Create("user-1", "user@test.com", "Test User", "user-1");

        _teamRepo.Setup(r => r.GetByIdAsync("home-1", default)).ReturnsAsync(homeTeam);
        _teamRepo.Setup(r => r.GetByIdAsync("away-1", default)).ReturnsAsync(awayTeam);
        _userRepo.Setup(r => r.GetByKeycloakIdAsync("user-1", default)).ReturnsAsync(user);
        _matchRepo.Setup(r => r.SaveAsync(It.IsAny<DomainMatch>(), default)).Returns(Task.CompletedTask);

        var command = new CreateMatchCommand("home-1", "away-1", "user-1");

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        Assert.NotEmpty(result);
        _matchRepo.Verify(r => r.SaveAsync(It.IsAny<DomainMatch>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_HomeTeamNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _teamRepo.Setup(r => r.GetByIdAsync("home-1", default)).ReturnsAsync((Team?)null);

        var command = new CreateMatchCommand("home-1", "away-1", "user-1");

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(() => CreateHandler().Handle(command, default));
    }

    [Fact]
    public async Task Handle_AwayTeamNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var homeTeam = Team.Create("home-1", "Home Team", "owner-1");
        _teamRepo.Setup(r => r.GetByIdAsync("home-1", default)).ReturnsAsync(homeTeam);
        _teamRepo.Setup(r => r.GetByIdAsync("away-1", default)).ReturnsAsync((Team?)null);

        var command = new CreateMatchCommand("home-1", "away-1", "user-1");

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(() => CreateHandler().Handle(command, default));
    }

    // TC-MATCH-004: Fail to create with duplicate teams
    [Fact]
    public async Task Handle_SameTeamIds_ThrowsInvalidOperationException()
    {
        // Arrange
        var team = Team.Create("team-1", "Team One", "owner-1");
        var user = User.Create("user-1", "user@test.com", "Test User", "user-1");

        _teamRepo.Setup(r => r.GetByIdAsync("team-1", default)).ReturnsAsync(team);
        _userRepo.Setup(r => r.GetByKeycloakIdAsync("user-1", default)).ReturnsAsync(user);

        var command = new CreateMatchCommand("team-1", "team-1", "user-1");

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateHandler().Handle(command, default));
    }

    [Fact]
    public async Task Handle_WithPlayerRosters_CreatesMatchWithPlayers()
    {
        // Arrange
        var homeTeam = Team.Create("home-1", "Home Team", "owner-1");
        var awayTeam = Team.Create("away-1", "Away Team", "owner-2");
        var user = User.Create("user-1", "user@test.com", "Test User", "user-1");

        _teamRepo.Setup(r => r.GetByIdAsync("home-1", default)).ReturnsAsync(homeTeam);
        _teamRepo.Setup(r => r.GetByIdAsync("away-1", default)).ReturnsAsync(awayTeam);
        _userRepo.Setup(r => r.GetByKeycloakIdAsync("user-1", default)).ReturnsAsync(user);

        DomainMatch? savedMatch = null;
        _matchRepo.Setup(r => r.SaveAsync(It.IsAny<DomainMatch>(), default))
            .Callback<DomainMatch, CancellationToken>((m, _) => savedMatch = m)
            .Returns(Task.CompletedTask);

        var homePlayers = new List<PlayerInput>
        {
            new("LeBron James", 23),
            new("Anthony Davis", 3),
        };
        var awayPlayers = new List<PlayerInput>
        {
            new("Jayson Tatum", 0),
        };

        var command = new CreateMatchCommand("home-1", "away-1", "user-1", homePlayers, awayPlayers);

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        Assert.NotEmpty(result);
        Assert.NotNull(savedMatch);
        Assert.Equal(2, savedMatch!.HomePlayers.Count);
        Assert.Equal(1, savedMatch.AwayPlayers.Count);
        Assert.Equal("LeBron James", savedMatch.HomePlayers[0].Name);
        Assert.Equal(23, savedMatch.HomePlayers[0].Number);
        Assert.Equal("Jayson Tatum", savedMatch.AwayPlayers[0].Name);
    }
}
