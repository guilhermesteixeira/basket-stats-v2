namespace BasketStats.Domain.Tests.ValueObjectTests;

using Domain.ValueObjects;

public class MatchIdTests
{
    [Fact]
    public void Create_WithValidId_Succeeds()
    {
        // Arrange
        var id = "match-123";

        // Act
        var matchId = new MatchId(id);

        // Assert
        Assert.Equal(id, matchId.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithEmptyOrNullId_Throws(string id)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new MatchId(id));
    }

    [Fact]
    public void CreateNew_GeneratesValidGuid()
    {
        // Act
        var matchId = MatchId.CreateNew();

        // Assert
        Assert.NotNull(matchId.Value);
        Assert.True(Guid.TryParse(matchId.Value, out _));
    }

    [Fact]
    public void CreateNew_GeneratesUniqueIds()
    {
        // Act
        var id1 = MatchId.CreateNew();
        var id2 = MatchId.CreateNew();

        // Assert
        Assert.NotEqual(id1.Value, id2.Value);
    }

    [Fact]
    public void TwoMatchIds_WithSameValue_AreEqual()
    {
        // Arrange
        var id = "match-123";
        var matchId1 = new MatchId(id);
        var matchId2 = new MatchId(id);

        // Act & Assert
        Assert.Equal(matchId1, matchId2);
    }

    [Fact]
    public void TwoMatchIds_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var matchId1 = new MatchId("match-1");
        var matchId2 = new MatchId("match-2");

        // Act & Assert
        Assert.NotEqual(matchId1, matchId2);
    }
}
