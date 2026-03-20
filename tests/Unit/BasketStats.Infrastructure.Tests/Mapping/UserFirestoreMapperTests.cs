using BasketStats.Domain.Entities;
using BasketStats.Infrastructure.Mapping;

namespace BasketStats.Infrastructure.Tests.Mapping;

public class UserFirestoreMapperTests
{
    [Fact]
    public void ToDocument_User_MapsAllProperties()
    {
        // Arrange
        var user = User.Create("user-1", "john@example.com", "John Doe", "kc-123");

        // Act
        var doc = UserFirestoreMapper.ToDocument(user);

        // Assert
        Assert.Equal(user.Id, doc.Id);
        Assert.Equal(user.Email, doc.Email);
        Assert.Equal(user.Name, doc.Name);
        Assert.Equal(user.KeycloakId, doc.KeycloakId);
        Assert.Equal(user.CreatedAt, doc.CreatedAt);
        Assert.Empty(doc.Roles);
    }

    [Fact]
    public void ToDocument_UserWithRoles_MapsRoles()
    {
        // Arrange
        var user = User.Create("user-1", "john@example.com", "John Doe", "kc-123");
        user.AddRole("admin");
        user.AddRole("match-creator");

        // Act
        var doc = UserFirestoreMapper.ToDocument(user);

        // Assert
        Assert.Equal(2, doc.Roles.Count);
        Assert.Contains("admin", doc.Roles);
        Assert.Contains("match-creator", doc.Roles);
    }

    [Fact]
    public void ToDomain_Document_ReconstructsUserCorrectly()
    {
        // Arrange
        var user = User.Create("user-1", "john@example.com", "John Doe", "kc-123");
        user.AddRole("admin");
        var doc = UserFirestoreMapper.ToDocument(user);

        // Act
        var result = UserFirestoreMapper.ToDomain(doc);

        // Assert
        Assert.Equal(doc.Id, result.Id);
        Assert.Equal(doc.Email, result.Email);
        Assert.Equal(doc.Name, result.Name);
        Assert.Equal(doc.KeycloakId, result.KeycloakId);
        Assert.Equal(doc.CreatedAt, result.CreatedAt);
    }

    [Fact]
    public void ToDomain_Document_RoundTrip_PreservesRoles()
    {
        // Arrange
        var original = User.Create("user-1", "jane@example.com", "Jane Doe", "kc-456");
        original.AddRole("admin");
        original.AddRole("match-creator");
        var doc = UserFirestoreMapper.ToDocument(original);

        // Act
        var result = UserFirestoreMapper.ToDomain(doc);

        // Assert
        Assert.Equal(2, result.Roles.Count);
        Assert.Contains("admin", result.Roles);
        Assert.Contains("match-creator", result.Roles);
    }
}
