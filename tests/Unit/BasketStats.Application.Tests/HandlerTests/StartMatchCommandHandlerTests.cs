namespace BasketStats.Application.Tests.HandlerTests;

using Moq;
using BasketStats.Application.Commands;
using BasketStats.Application.Exceptions;
using BasketStats.Application.Handlers;
using BasketStats.Domain.Abstractions;
using BasketStats.Domain.Entities;

public class StartMatchCommandHandlerTests
{
    private readonly Mock<IMatchRepository> _matchRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();

    private StartMatchCommandHandler CreateHandler() =>
        new(_matchRepo.Object, _userRepo.Object);

    [Fact]
    public async Task Handle_ScheduledMatch_StartsMatch()
    {
        var match = DomainMatch.Create("home-1", "away-1");
        var user = User.Create("user-1", "user@test.com", "Test User", "kc-1");

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);
        _userRepo.Setup(r => r.GetByIdAsync("user-1", default)).ReturnsAsync(user);
        _matchRepo.Setup(r => r.SaveAsync(match, default)).Returns(Task.CompletedTask);

        var command = new StartMatchCommand(match.Id.Value, "user-1");
        await CreateHandler().Handle(command, default);

        Assert.Equal(Domain.Enums.MatchStatus.Active, match.Status);
        _matchRepo.Verify(r => r.SaveAsync(match, default), Times.Once);
    }

    [Fact]
    public async Task Handle_MatchNotFound_ThrowsNotFoundException()
    {
        _matchRepo.Setup(r => r.GetByIdAsync("no-match", default)).ReturnsAsync((DomainMatch?)null);

        var command = new StartMatchCommand("no-match", "user-1");

        await Assert.ThrowsAsync<NotFoundException>(() => CreateHandler().Handle(command, default));
    }

    [Fact]
    public async Task Handle_ActiveMatch_ThrowsInvalidOperationException()
    {
        var match = DomainMatch.Create("home-1", "away-1");
        match.Start(); // already active
        var user = User.Create("user-1", "user@test.com", "Test User", "kc-1");

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);
        _userRepo.Setup(r => r.GetByIdAsync("user-1", default)).ReturnsAsync(user);

        var command = new StartMatchCommand(match.Id.Value, "user-1");

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateHandler().Handle(command, default));
    }
}
