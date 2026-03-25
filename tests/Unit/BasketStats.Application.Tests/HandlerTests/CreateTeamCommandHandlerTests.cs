namespace BasketStats.Application.Tests.HandlerTests;

using Moq;
using BasketStats.Application.Commands;
using BasketStats.Application.Exceptions;
using BasketStats.Application.Handlers;
using BasketStats.Domain.Abstractions;
using BasketStats.Domain.Entities;

public class CreateTeamCommandHandlerTests
{
    private readonly Mock<ITeamRepository> _teamRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();

    private CreateTeamCommandHandler CreateHandler() =>
        new(_teamRepo.Object, _userRepo.Object);

    [Fact]
    public async Task Handle_ValidCommand_CreatesTeamAndReturnsId()
    {
        // Arrange
        var user = User.Create("user-1", "user@test.com", "Test User", "kc-user-1");
        _userRepo.Setup(r => r.GetByKeycloakIdAsync("kc-user-1", default)).ReturnsAsync(user);
        _teamRepo.Setup(r => r.SaveAsync(It.IsAny<Team>(), default)).Returns(Task.CompletedTask);

        var command = new CreateTeamCommand("Lakers", "kc-user-1");

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        Assert.NotEmpty(result);
        _teamRepo.Verify(r => r.SaveAsync(It.Is<Team>(t =>
            t.Name == "Lakers" && t.OwnerId == "user-1"), default), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _userRepo.Setup(r => r.GetByKeycloakIdAsync("kc-unknown", default)).ReturnsAsync((User?)null);

        var command = new CreateTeamCommand("Lakers", "kc-unknown");

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(() => CreateHandler().Handle(command, default));
    }

    [Fact]
    public async Task Handle_CreatedTeam_HasOwnerSetToFirestoreUserId()
    {
        // Arrange — internal Firestore ID is different from Keycloak sub
        var user = User.Create("firestore-id-abc", "user@test.com", "Test User", "kc-user-1");
        _userRepo.Setup(r => r.GetByKeycloakIdAsync("kc-user-1", default)).ReturnsAsync(user);

        Team? savedTeam = null;
        _teamRepo.Setup(r => r.SaveAsync(It.IsAny<Team>(), default))
            .Callback<Team, CancellationToken>((t, _) => savedTeam = t)
            .Returns(Task.CompletedTask);

        var command = new CreateTeamCommand("Celtics", "kc-user-1");

        // Act
        await CreateHandler().Handle(command, default);

        // Assert — OwnerId must be the Firestore user ID, not the Keycloak sub
        Assert.NotNull(savedTeam);
        Assert.Equal("firestore-id-abc", savedTeam.OwnerId);
        Assert.NotEqual("kc-user-1", savedTeam.OwnerId);
    }

    [Fact]
    public async Task Handle_MultipleTeams_SameOwner_EachTeamHasDistinctId()
    {
        // Arrange
        var user = User.Create("user-1", "user@test.com", "Test User", "kc-user-1");
        _userRepo.Setup(r => r.GetByKeycloakIdAsync("kc-user-1", default)).ReturnsAsync(user);

        var savedTeams = new List<Team>();
        _teamRepo.Setup(r => r.SaveAsync(It.IsAny<Team>(), default))
            .Callback<Team, CancellationToken>((t, _) => savedTeams.Add(t))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        // Act
        var id1 = await handler.Handle(new CreateTeamCommand("Lakers", "kc-user-1"), default);
        var id2 = await handler.Handle(new CreateTeamCommand("Bulls", "kc-user-1"), default);

        // Assert
        Assert.NotEqual(id1, id2);
        Assert.Equal(2, savedTeams.Count);
    }
}
