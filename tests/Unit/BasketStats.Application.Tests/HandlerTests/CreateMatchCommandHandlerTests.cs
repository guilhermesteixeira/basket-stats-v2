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

    [Fact]
    public async Task Handle_ValidCommand_CreatesMatchAndReturnsId()
    {
        var homeTeam = Team.Create("home-1", "Home Team", "owner-1");
        var awayTeam = Team.Create("away-1", "Away Team", "owner-2");
        var user = User.Create("user-1", "user@test.com", "Test User", "kc-1");

        _teamRepo.Setup(r => r.GetByIdAsync("home-1", default)).ReturnsAsync(homeTeam);
        _teamRepo.Setup(r => r.GetByIdAsync("away-1", default)).ReturnsAsync(awayTeam);
        _userRepo.Setup(r => r.GetByIdAsync("user-1", default)).ReturnsAsync(user);
        _matchRepo.Setup(r => r.SaveAsync(It.IsAny<DomainMatch>(), default)).Returns(Task.CompletedTask);

        var command = new CreateMatchCommand("home-1", "away-1", "user-1");
        var handler = CreateHandler();

        var result = await handler.Handle(command, default);

        Assert.NotEmpty(result);
        _matchRepo.Verify(r => r.SaveAsync(It.IsAny<DomainMatch>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_HomeTeamNotFound_ThrowsNotFoundException()
    {
        _teamRepo.Setup(r => r.GetByIdAsync("home-1", default)).ReturnsAsync((Team?)null);

        var command = new CreateMatchCommand("home-1", "away-1", "user-1");
        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, default));
    }

    [Fact]
    public async Task Handle_AwayTeamNotFound_ThrowsNotFoundException()
    {
        var homeTeam = Team.Create("home-1", "Home Team", "owner-1");
        _teamRepo.Setup(r => r.GetByIdAsync("home-1", default)).ReturnsAsync(homeTeam);
        _teamRepo.Setup(r => r.GetByIdAsync("away-1", default)).ReturnsAsync((Team?)null);

        var command = new CreateMatchCommand("home-1", "away-1", "user-1");
        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, default));
    }
}
