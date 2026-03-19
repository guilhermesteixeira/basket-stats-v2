namespace BasketStats.Domain.Tests.EntityTests;

using Domain.Entities;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        // Act
        var user = User.Create("user-1", "john@example.com", "John Doe", "keycloak-id-123");

        // Assert
        Assert.Equal("user-1", user.Id);
        Assert.Equal("john@example.com", user.Email);
        Assert.Equal("John Doe", user.Name);
        Assert.Equal("keycloak-id-123", user.KeycloakId);
        Assert.Empty(user.Roles);
    }

    [Theory]
    [InlineData("", "john@example.com", "John Doe", "keycloak-id-123")]
    [InlineData("user-1", "", "John Doe", "keycloak-id-123")]
    [InlineData("user-1", "john@example.com", "", "keycloak-id-123")]
    [InlineData("user-1", "john@example.com", "John Doe", "")]
    public void Create_WithEmptyField_Throws(string id, string email, string name, string keycloakId)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => User.Create(id, email, name, keycloakId));
    }

    [Fact]
    public void AddRole_WithValidRole_Succeeds()
    {
        // Arrange
        var user = User.Create("user-1", "john@example.com", "John Doe", "keycloak-id-123");

        // Act
        user.AddRole("match-creator");

        // Assert
        Assert.Single(user.Roles);
        Assert.Contains("match-creator", user.Roles);
    }

    [Fact]
    public void AddRole_WithDuplicateRole_DoesNotAddDuplicate()
    {
        // Arrange
        var user = User.Create("user-1", "john@example.com", "John Doe", "keycloak-id-123");
        user.AddRole("admin");

        // Act
        user.AddRole("admin");

        // Assert
        Assert.Single(user.Roles);
    }

    [Fact]
    public void AddRole_WithEmptyRole_Throws()
    {
        // Arrange
        var user = User.Create("user-1", "john@example.com", "John Doe", "keycloak-id-123");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => user.AddRole(""));
    }

    [Fact]
    public void HasRole_WithExistingRole_ReturnsTrue()
    {
        // Arrange
        var user = User.Create("user-1", "john@example.com", "John Doe", "keycloak-id-123");
        user.AddRole("match-creator");

        // Act
        var hasRole = user.HasRole("match-creator");

        // Assert
        Assert.True(hasRole);
    }

    [Fact]
    public void HasRole_WithNonExistingRole_ReturnsFalse()
    {
        // Arrange
        var user = User.Create("user-1", "john@example.com", "John Doe", "keycloak-id-123");

        // Act
        var hasRole = user.HasRole("admin");

        // Assert
        Assert.False(hasRole);
    }

    [Fact]
    public void IsMatchCreator_WithMatchCreatorRole_ReturnsTrue()
    {
        // Arrange
        var user = User.Create("user-1", "john@example.com", "John Doe", "keycloak-id-123");
        user.AddRole("match-creator");

        // Act
        var isMatchCreator = user.IsMatchCreator;

        // Assert
        Assert.True(isMatchCreator);
    }

    [Fact]
    public void IsMatchCreator_WithoutRole_ReturnsFalse()
    {
        // Arrange
        var user = User.Create("user-1", "john@example.com", "John Doe", "keycloak-id-123");

        // Act
        var isMatchCreator = user.IsMatchCreator;

        // Assert
        Assert.False(isMatchCreator);
    }

    [Fact]
    public void IsAdmin_WithAdminRole_ReturnsTrue()
    {
        // Arrange
        var user = User.Create("user-1", "john@example.com", "John Doe", "keycloak-id-123");
        user.AddRole("admin");

        // Act
        var isAdmin = user.IsAdmin;

        // Assert
        Assert.True(isAdmin);
    }

    [Fact]
    public void IsAdmin_WithoutRole_ReturnsFalse()
    {
        // Arrange
        var user = User.Create("user-1", "john@example.com", "John Doe", "keycloak-id-123");

        // Act
        var isAdmin = user.IsAdmin;

        // Assert
        Assert.False(isAdmin);
    }

    [Fact]
    public void User_WithMultipleRoles_Succeeds()
    {
        // Arrange
        var user = User.Create("user-1", "john@example.com", "John Doe", "keycloak-id-123");

        // Act
        user.AddRole("match-creator");
        user.AddRole("admin");
        user.AddRole("match-viewer");

        // Assert
        Assert.Equal(3, user.Roles.Count);
        Assert.True(user.IsMatchCreator);
        Assert.True(user.IsAdmin);
        Assert.True(user.HasRole("match-viewer"));
    }
}
