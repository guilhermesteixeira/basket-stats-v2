namespace BasketStats.Application.Tests.HandlerTests;

using Moq;
using BasketStats.Application.Commands;
using BasketStats.Application.Exceptions;
using BasketStats.Application.Handlers;
using BasketStats.Domain.Abstractions;

public class FinishMatchCommandHandlerTests
{
    private readonly Mock<IMatchRepository> _matchRepo = new();

    private FinishMatchCommandHandler CreateHandler() =>
        new(_matchRepo.Object);

    [Fact]
    public async Task Handle_ActiveMatch_FinishesMatch()
    {
        var match = DomainMatch.Create("home-1", "away-1");
        match.Start();

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);
        _matchRepo.Setup(r => r.SaveAsync(match, default)).Returns(Task.CompletedTask);

        var command = new FinishMatchCommand(match.Id.Value, "user-1");
        await CreateHandler().Handle(command, default);

        Assert.Equal(Domain.Enums.MatchStatus.Finished, match.Status);
        _matchRepo.Verify(r => r.SaveAsync(match, default), Times.Once);
    }

    [Fact]
    public async Task Handle_MatchNotFound_ThrowsNotFoundException()
    {
        _matchRepo.Setup(r => r.GetByIdAsync("no-match", default)).ReturnsAsync((DomainMatch?)null);

        var command = new FinishMatchCommand("no-match", "user-1");

        await Assert.ThrowsAsync<NotFoundException>(() => CreateHandler().Handle(command, default));
    }

    [Fact]
    public async Task Handle_ScheduledMatch_ThrowsInvalidOperationException()
    {
        var match = DomainMatch.Create("home-1", "away-1"); // still Scheduled

        _matchRepo.Setup(r => r.GetByIdAsync(match.Id.Value, default)).ReturnsAsync(match);

        var command = new FinishMatchCommand(match.Id.Value, "user-1");

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateHandler().Handle(command, default));
    }
}
