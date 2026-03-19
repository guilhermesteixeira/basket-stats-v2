namespace BasketStats.Domain.Tests.EntityTests;

using Domain.Entities;

public class TeamTests
{
    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        // Act
        var team = Team.Create("team-1", "Lakers", "owner-1");

        // Assert
        Assert.Equal("team-1", team.Id);
        Assert.Equal("Lakers", team.Name);
        Assert.Equal("owner-1", team.OwnerId);
    }

    [Theory]
    [InlineData("", "Lakers", "owner-1")]
    [InlineData("team-1", "", "owner-1")]
    [InlineData("team-1", "Lakers", "")]
    public void Create_WithEmptyField_Throws(string id, string name, string ownerId)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Team.Create(id, name, ownerId));
    }

    [Fact]
    public void IsOwnedBy_WithCorrectOwner_ReturnsTrue()
    {
        // Arrange
        var team = Team.Create("team-1", "Lakers", "owner-1");

        // Act
        var isOwned = team.IsOwnedBy("owner-1");

        // Assert
        Assert.True(isOwned);
    }

    [Fact]
    public void IsOwnedBy_WithDifferentOwner_ReturnsFalse()
    {
        // Arrange
        var team = Team.Create("team-1", "Lakers", "owner-1");

        // Act
        var isOwned = team.IsOwnedBy("owner-2");

        // Assert
        Assert.False(isOwned);
    }
}
