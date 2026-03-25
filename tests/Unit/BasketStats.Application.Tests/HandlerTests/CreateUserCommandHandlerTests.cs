namespace BasketStats.Application.Tests.HandlerTests;

using Moq;
using BasketStats.Application.Commands;
using BasketStats.Application.Handlers;
using BasketStats.Domain.Abstractions;
using BasketStats.Domain.Entities;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();

    private CreateUserCommandHandler CreateHandler() =>
        new(_userRepo.Object);

    [Fact]
    public async Task Handle_NewUser_CreatesAndReturnsId()
    {
        // Arrange
        _userRepo.Setup(r => r.GetByKeycloakIdAsync("kc-user-1", default)).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.SaveAsync(It.IsAny<User>(), default)).Returns(Task.CompletedTask);

        var command = new CreateUserCommand("user@test.com", "Test User", "kc-user-1");

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        Assert.NotEmpty(result);
        _userRepo.Verify(r => r.SaveAsync(It.Is<User>(u =>
            u.Email == "user@test.com" &&
            u.Name == "Test User" &&
            u.KeycloakId == "kc-user-1"), default), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingUser_ReturnsExistingId_DoesNotSave()
    {
        // Arrange
        var existing = User.Create("existing-id", "user@test.com", "Test User", "kc-user-1");
        _userRepo.Setup(r => r.GetByKeycloakIdAsync("kc-user-1", default)).ReturnsAsync(existing);

        var command = new CreateUserCommand("user@test.com", "Test User", "kc-user-1");

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert — returns existing ID without creating a new user
        Assert.Equal("existing-id", result);
        _userRepo.Verify(r => r.SaveAsync(It.IsAny<User>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_NewUser_InternalIdIsDifferentFromKeycloakId()
    {
        // Arrange
        User? savedUser = null;
        _userRepo.Setup(r => r.GetByKeycloakIdAsync("kc-abc-123", default)).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.SaveAsync(It.IsAny<User>(), default))
            .Callback<User, CancellationToken>((u, _) => savedUser = u)
            .Returns(Task.CompletedTask);

        var command = new CreateUserCommand("user@test.com", "Test User", "kc-abc-123");

        // Act
        var returnedId = await CreateHandler().Handle(command, default);

        // Assert — internal Firestore ID is a new GUID, not the Keycloak sub
        Assert.NotNull(savedUser);
        Assert.Equal("kc-abc-123", savedUser.KeycloakId);
        Assert.NotEqual("kc-abc-123", savedUser.Id);
        Assert.Equal(returnedId, savedUser.Id);
    }
}
