namespace BasketStats.Integration.Tests;

using BasketStats.Domain.Entities;
using BasketStats.Infrastructure.Repositories;

[Trait("Category", "Integration")]
public class UserRepositoryIntegrationTests : IClassFixture<FirestoreIntegrationFixture>
{
    private readonly FirestoreUserRepository? _repo;
    private readonly bool _available;

    public UserRepositoryIntegrationTests(FirestoreIntegrationFixture fixture)
    {
        _available = fixture.IsAvailable;
        if (fixture.IsAvailable)
            _repo = new FirestoreUserRepository(fixture.FirestoreDb!);
    }

    [SkippableFact]
    public async Task SaveAsync_ThenGetByIdAsync_RoundTrip_ReturnsUser()
    {
        Skip.IfNot(_available, "Firestore emulator not available");

        var user = User.Create("user-id-1", "alice@example.com", "Alice", "keycloak-id-1");

        await _repo!.SaveAsync(user);
        var retrieved = await _repo.GetByIdAsync(user.Id);

        Assert.NotNull(retrieved);
        Assert.Equal(user.Id, retrieved.Id);
        Assert.Equal("alice@example.com", retrieved.Email);
        Assert.Equal("Alice", retrieved.Name);
        Assert.Equal("keycloak-id-1", retrieved.KeycloakId);
    }

    [SkippableFact]
    public async Task GetByKeycloakIdAsync_ReturnsCorrectUser()
    {
        Skip.IfNot(_available, "Firestore emulator not available");

        var user = User.Create("user-id-2", "bob@example.com", "Bob", "keycloak-id-2");
        await _repo!.SaveAsync(user);

        var retrieved = await _repo.GetByKeycloakIdAsync("keycloak-id-2");

        Assert.NotNull(retrieved);
        Assert.Equal(user.Id, retrieved.Id);
        Assert.Equal("bob@example.com", retrieved.Email);
    }

    [SkippableFact]
    public async Task GetByEmailAsync_ReturnsCorrectUser()
    {
        Skip.IfNot(_available, "Firestore emulator not available");

        var user = User.Create("user-id-3", "carol@example.com", "Carol", "keycloak-id-3");
        await _repo!.SaveAsync(user);

        var retrieved = await _repo.GetByEmailAsync("carol@example.com");

        Assert.NotNull(retrieved);
        Assert.Equal(user.Id, retrieved.Id);
        Assert.Equal("Carol", retrieved.Name);
    }

    [SkippableFact]
    public async Task SaveAsync_UserWithRoles_PersistsRoles()
    {
        Skip.IfNot(_available, "Firestore emulator not available");

        var user = User.Create("user-id-4", "dave@example.com", "Dave", "keycloak-id-4");
        user.AddRole("admin");
        user.AddRole("match-creator");

        await _repo!.SaveAsync(user);
        var retrieved = await _repo.GetByIdAsync(user.Id);

        Assert.NotNull(retrieved);
        Assert.Contains("admin", retrieved.Roles);
        Assert.Contains("match-creator", retrieved.Roles);
    }
}
